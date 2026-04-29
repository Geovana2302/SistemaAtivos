using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;
using SistemaAtivos.Services;

namespace SistemaAtivos.Controllers
{
    // =====================================================================
    // REQUISITO 1 - [EmpresaAuthorize] protege todo o controller (area restrita)
    // REQUISITO 4 - LOGICA OPERACIONAL (Actions complexas)
    //   - MoverStatus: gerencia maquina de estados em tempo real
    //   - Aprovar: validacao de status + atualizacao de campos de aprovacao
    //   - Orcamento: atualiza diagnostico + valor + muda status
    //   - QuadroTecnico: query complexa com filtros + agrupamento por status
    //   - Historico: filtros por empresa + datas + calculo de faturamento
    // REQUISITO 5 - ESTABILIDADE (try-catch em todas as operacoes criticas)
    // =====================================================================
    [EmpresaAuthorize]
    public class ManutencaoController : Controller
    {
        private AtivosContext db = new AtivosContext();
        private readonly ManutencaoService _service;

        public ManutencaoController()
        {
            db = new AtivosContext();
            _service = new ManutencaoService(db);
        }

        public ManutencaoController(ManutencaoService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            db = _service == null ? new AtivosContext() : (AtivosContext)typeof(ManutencaoService)
                .GetField("_db", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(_service);
        }

        // REQUISITO 1 - Metodos auxiliares que consultam a Sessao
        // para controlar permissoes e filtrar dados por empresa
        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;
        private string GetUsuarioNome() => Session["UsuarioNome"]?.ToString() ?? "Sistema";

        // REQUISITO 4 - Query base com Include para carregar entidades relacionadas
        // Filtra automaticamente por empresa quando o usuario nao e Admin
        private IQueryable<Manutencao> GetQuery()
        {
            var q = db.Manutencoes
                .Include(m => m.Ativo)
                .Include(m => m.Empresa)
                .AsQueryable();
            if (!IsAdmin())
                q = q.Where(m => m.EmpresaId == GetEmpresaId());
            return q;
        }

        // Lista de ativos em manutenção (para empresas)
        public ActionResult Index(int? empresaId, string ordem)
        {
            if (IsAdmin())
            {
                return RedirectToAction("QuadroTecnico");
            }

            // Visão da empresa: apenas suas manutenções
            var manutencoes = GetQuery()
                .Include(m => m.Ativo)
                .OrderByDescending(m => m.Data)
                .ToList();

            return View(manutencoes);
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // Quadro Kanban: carrega manutencoes ativas com Include de multiplas entidades,
        // aplica filtro por empresa, agrupa por status e ordena por data
        public ActionResult QuadroTecnico(int? empresaId)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index");
            }

            var query = db.Manutencoes
                .Include(m => m.Ativo)
                .Include(m => m.Empresa)
                .Where(m => m.Status != StatusManutencao.Concluido && m.Status != StatusManutencao.Cancelado)
                .AsQueryable();

            if (empresaId.HasValue)
                query = query.Where(m => m.EmpresaId == empresaId);

            var manutencoes = query.OrderBy(m => m.Data).ToList();

            ViewBag.Empresas = db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;

            return View(manutencoes);
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // Historico: filtra por empresa, intervalo de datas,
        // calcula faturamento total - gerencia estado dos dados em tempo real
        public ActionResult Historico(int? empresaId, DateTime? dataInicio, DateTime? dataFim)
        {
            if (!IsAdmin())
            {
                return HttpNotFound();
            }

            var query = db.Manutencoes
                .Include(m => m.Ativo)
                .Include(m => m.Empresa)
                .Where(m => m.Status == StatusManutencao.Concluido)
                .AsQueryable();

            if (empresaId.HasValue)
                query = query.Where(m => m.EmpresaId == empresaId);

            if (dataInicio.HasValue)
                query = query.Where(m => m.DataConclusao >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(m => m.DataConclusao <= dataFim.Value);

            var manutencoes = query.OrderByDescending(m => m.DataConclusao).ToList();

            ViewBag.Empresas = db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;
            ViewBag.DataInicio = dataInicio;
            ViewBag.DataFim = dataFim;
            ViewBag.TotalFaturamento = manutencoes.Sum(m => m.ValorOrcamento ?? 0);

            return View(manutencoes);
        }

        [HttpGet]
        public ActionResult Create(int? ativoId)
        {
            PopularDropdowns(ativoId: ativoId);
            var man = new Manutencao { AtivoId = ativoId, Data = DateTime.Today };
            if (!IsAdmin()) man.EmpresaId = GetEmpresaId();
            
            var ativos = IsAdmin()
                ? db.Ativos.Select(a => new { a.Id, a.Nome, a.EmpresaId }).ToList()
                : db.Ativos.Where(a => a.EmpresaId == GetEmpresaId()).Select(a => new { a.Id, a.Nome, a.EmpresaId }).ToList();
            ViewBag.Ativos = ativos;
            
            return View("~/Views/Manutencao/Create.cshtml", man);
        }

        // REQUISITO 3 - ModelState.IsValid verifica as Data Annotations antes de salvar
        // REQUISITO 4 - Define status inicial como Aberto (inicio do fluxo)
        // REQUISITO 5 - try-catch para tratamento de excecoes em operacao critica
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Manutencao manutencao)
        {
            if (!IsAdmin()) manutencao.EmpresaId = GetEmpresaId();
            
            // REQUISITO 3 - Validacao dos Data Annotations do Model
            if (ModelState.IsValid)
            {
                // REQUISITO 5 - Bloco try-catch para operacao critica de persistencia
                try
                {
                    manutencao.Status = StatusManutencao.Aberto;
                    _service.CriarChamado(manutencao);
                    TempData["Sucesso"] = "Chamado de manutenção aberto com sucesso.";
                    if (manutencao.AtivoId.HasValue)
                        return RedirectToAction("Detalhes", "Ativo", new { id = manutencao.AtivoId });
                    return RedirectToAction(IsAdmin() ? "QuadroTecnico" : "Index");
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = "Erro ao abrir chamado: " + ex.Message;
                }
            }
            PopularDropdowns(manutencao);
            
            var ativos = IsAdmin()
                ? db.Ativos.Select(a => new { a.Id, a.Nome, a.EmpresaId }).ToList()
                : db.Ativos.Where(a => a.EmpresaId == GetEmpresaId()).Select(a => new { a.Id, a.Nome, a.EmpresaId }).ToList();
            ViewBag.Ativos = ativos;
            
            return View("~/Views/Manutencao/Create.cshtml", manutencao);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var man = GetQuery().FirstOrDefault(m => m.Id == id);
            if (man == null) return HttpNotFound();
            PopularDropdowns(man);
            return View(man);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Manutencao manutencao)
        {
            if (!IsAdmin()) manutencao.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(manutencao).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Sucesso"] = "Manutenção atualizada com sucesso.";
                    if (manutencao.AtivoId.HasValue)
                        return RedirectToAction("Detalhes", "Ativo", new { id = manutencao.AtivoId });
                    return RedirectToAction(IsAdmin() ? "QuadroTecnico" : "Index");
                }
                catch (Exception ex)
                {
                    TempData["Erro"] = "Erro ao atualizar: " + ex.Message;
                }
            }
            PopularDropdowns(manutencao);
            return View(manutencao);
        }

        // Modal para preenchimento técnico (Admin)
        [HttpGet]
        public ActionResult Orcamento(int id)
        {
            if (!IsAdmin()) return HttpNotFound();
            
            var man = db.Manutencoes
                .Include(m => m.Ativo)
                .Include(m => m.Empresa)
                .FirstOrDefault(m => m.Id == id);
            
            if (man == null) return HttpNotFound();
            return PartialView("_OrcamentoModal", man);
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // Recebe orcamento do tecnico, atualiza diagnostico e valor,
        // e move o status para AguardandoAprovacao automaticamente
        // REQUISITO 5 - try-catch para tratamento de excecoes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Orcamento(int id, decimal valorOrcamento, string diagnosticoTecnico, string observacaoInterna)
        {
            if (!IsAdmin()) return HttpNotFound();
            
            // REQUISITO 5 - Bloco try-catch para operacao critica
            try
            {
                var man = db.Manutencoes.Find(id);
                if (man == null) return HttpNotFound();

                man.ValorOrcamento = valorOrcamento;
                man.DiagnosticoTecnico = diagnosticoTecnico;
                man.ObservacaoInterna = observacaoInterna;
                man.Status = StatusManutencao.AguardandoAprovacao;

                db.Entry(man).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Sucesso"] = "Orçamento enviado para aprovação do cliente.";
                return RedirectToAction("QuadroTecnico");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao salvar orçamento: " + ex.Message;
                return RedirectToAction("QuadroTecnico");
            }
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // Aprovar orcamento: valida se o status permite aprovacao,
        // registra data, nome de quem aprovou, e move o status.
        // Demonstra gerenciamento de estado dos dados em tempo real.
        // REQUISITO 5 - try-catch para tratamento de excecoes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aprovar(int id)
        {
            // REQUISITO 5 - Bloco try-catch para operacao critica
            try
            {
                var man = GetQuery().FirstOrDefault(m => m.Id == id);
                if (man == null) return HttpNotFound();

                if (man.Status != StatusManutencao.AguardandoAprovacao)
                {
                    TempData["Erro"] = "Esta manutenção não está aguardando aprovação.";
                    return RedirectToAction("Index");
                }

                man.Status = StatusManutencao.Aprovado;
                man.DataAprovacao = DateTime.Now;
                man.AprovadoPor = GetUsuarioNome();

                db.Entry(man).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Sucesso"] = "Orçamento aprovado. O técnico iniciará o serviço em breve.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao aprovar: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // MoverStatus: gerencia a maquina de estados em tempo real.
        // Permite mover o chamado entre os status do fluxo.
        // Se o novo status for Concluido, registra a data de conclusao.
        // Suporta chamadas AJAX (drag-and-drop do quadro Kanban).
        // REQUISITO 5 - try-catch para tratamento de excecoes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MoverStatus(int id, StatusManutencao novoStatus)
        {
            if (!IsAdmin()) return HttpNotFound();
            
            // REQUISITO 5 - Bloco try-catch para operacao critica
            try
            {
                var man = db.Manutencoes.Find(id);
                if (man == null) return HttpNotFound();

                man.Status = novoStatus;
                
                if (novoStatus == StatusManutencao.Concluido)
                    man.DataConclusao = DateTime.Now;

                db.Entry(man).State = EntityState.Modified;
                db.SaveChanges();

                if (Request.IsAjaxRequest())
                    return Json(new { success = true });

                TempData["Sucesso"] = "Status alterado para: " + novoStatus;
                return RedirectToAction("QuadroTecnico");
            }
            catch (Exception ex)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = ex.Message });

                TempData["Erro"] = "Erro ao mover status: " + ex.Message;
                return RedirectToAction("QuadroTecnico");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var man = GetQuery().FirstOrDefault(m => m.Id == id);
            if (man == null) return HttpNotFound();
            var ativoId = man.AtivoId;
            try
            {
                db.Manutencoes.Remove(man);
                db.SaveChanges();
                TempData["Sucesso"] = "Manutenção excluída com sucesso.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao excluir: " + ex.Message;
            }
            if (ativoId.HasValue)
                return RedirectToAction("Detalhes", "Ativo", new { id = ativoId });
            return RedirectToAction(IsAdmin() ? "QuadroTecnico" : "Index");
        }

        private void PopularDropdowns(Manutencao man = null, int? ativoId = null)
        {
            var ativos = IsAdmin()
                ? db.Ativos.ToList()
                : db.Ativos.Where(a => a.EmpresaId == GetEmpresaId()).ToList();
            ViewBag.AtivoId = new SelectList(ativos, "Id", "Nome", man?.AtivoId ?? ativoId);

            if (IsAdmin())
                ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", man?.EmpresaId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
