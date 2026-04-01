using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class ManutencaoController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

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

        public ActionResult Index(int? empresaId, string ordem)
        {
            var q = db.Ativos
                .Include(a => a.Categoria)
                .Include(a => a.Colaborador)
                .Include(a => a.Empresa)
                .Where(a => a.Status == StatusAtivo.EmManutencao);

            if (!IsAdmin())
                q = q.Where(a => a.EmpresaId == GetEmpresaId());

            if (empresaId.HasValue)
                q = q.Where(a => a.EmpresaId == empresaId);

            switch (ordem)
            {
                case "antigo":
                    q = q.OrderBy(a => a.Id);
                    break;
                case "empresa":
                    q = q.OrderBy(a => a.Empresa.Nome).ThenByDescending(a => a.Id);
                    break;
                default:
                    q = q.OrderByDescending(a => a.Id);
                    break;
            }

            ViewBag.Empresas = db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;
            ViewBag.OrdemAtual = ordem ?? "recente";
            return View(q.ToList());
        }

        [HttpGet]
        public ActionResult Create(int? ativoId)
        {
            PopularDropdowns(ativoId: ativoId);
            var man = new Manutencao { AtivoId = ativoId };
            if (!IsAdmin()) man.EmpresaId = GetEmpresaId();
            return View(man);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Manutencao manutencao)
        {
            if (!IsAdmin()) manutencao.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Manutencoes.Add(manutencao);
                    db.SaveChanges();
                    TempData["Sucesso"] = "Manutenção registrada.";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao registrar manutenção. Tente novamente.";
                }
            }
            PopularDropdowns(manutencao);
            return View(manutencao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var man = GetQuery().FirstOrDefault(m => m.Id == id);
            if (man == null) return HttpNotFound();
            try
            {
                db.Manutencoes.Remove(man);
                db.SaveChanges();
                TempData["Sucesso"] = "Manutenção excluída.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir manutenção. Tente novamente.";
            }
            return RedirectToAction("Index");
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
