using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class AtivoController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

        private IQueryable<Ativo> GetQuery()
        {
            var q = db.Ativos
                .Include(a => a.Categoria)
                .Include(a => a.Colaborador)
                .Include(a => a.Empresa)
                .AsQueryable();
            if (!IsAdmin())
            {
                var empresaId = GetEmpresaId();
                q = q.Where(a => a.EmpresaId == empresaId);
            }
            return q;
        }

        public ActionResult Index(int? empresaId, string ordem)
        {
            var q = AplicarFiltroEOrdem(GetQuery().Where(a => a.Status == StatusAtivo.Ativo), empresaId, ordem);
            PopularViewBagLista(empresaId, ordem);
            return View(q.ToList());
        }

        public ActionResult Detalhes(int id)
        {
            var ativo = GetQuery()
                .FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();
            return View(ativo);
        }

        public ActionResult Imprimir(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();
            return View(ativo);
        }

        public ActionResult QrCode(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();

            var scheme = Request.Url?.Scheme ?? "https";
            var url = Url.Action("Detalhes", "Ativo", new { id }, scheme);
            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCoder.PngByteQRCode(qrData);
                byte[] bytes = qrCode.GetGraphic(6);
                return File(bytes, "image/png");
            }
        }

        [HttpGet]
        public ActionResult Create(int? categoriaId, int? empresaId)
        {
            var ativo = new Ativo();
            if (categoriaId.HasValue) ativo.CategoriaId = categoriaId;
            if (empresaId.HasValue) ativo.EmpresaId = empresaId;
            else if (!IsAdmin()) ativo.EmpresaId = GetEmpresaId();
            PopularDropdowns(ativo);
            ViewBag.EmpresaPreSelecionada = empresaId.HasValue || !IsAdmin();
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Ativo ativo)
        {
            if (!IsAdmin()) ativo.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Ativos.Add(ativo);
                    db.SaveChanges();
                    TempData["Sucesso"] = "Ativo cadastrado com sucesso.";
                    if (ativo.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = ativo.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao cadastrar ativo. Tente novamente.";
                }
            }
            PopularDropdowns(ativo);
            return View(ativo);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();
            PopularDropdowns(ativo);
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Ativo ativo)
        {
            if (!IsAdmin()) ativo.EmpresaId = GetEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(ativo).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Sucesso"] = "Ativo atualizado.";
                    if (ativo.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = ativo.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao atualizar ativo. Tente novamente.";
                }
            }

            PopularDropdowns(ativo);
            return View(ativo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                TempData["Erro"] = "Apenas administradores podem excluir ativos.";
                return RedirectToAction("Index");
            }

            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();
            var empId = ativo.EmpresaId;

            try
            {
                db.Ativos.Remove(ativo);
                db.SaveChanges();
                TempData["Sucesso"] = "Ativo excluído.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir ativo. Tente novamente.";
            }
            if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }

        public ActionResult Inativos(int? empresaId, string ordem)
        {
            var q = AplicarFiltroEOrdem(GetQuery().Where(a => a.Status == StatusAtivo.Inativo), empresaId, ordem);
            PopularViewBagLista(empresaId, ordem);
            return View(q.ToList());
        }

        private IQueryable<Ativo> AplicarFiltroEOrdem(IQueryable<Ativo> q, int? empresaId, string ordem)
        {
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
            return q;
        }

        private void PopularViewBagLista(int? empresaId, string ordem)
        {
            ViewBag.Empresas = db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;
            ViewBag.OrdemAtual = ordem ?? "recente";
        }

        private void PopularDropdowns(Ativo ativo = null)
        {
            var empId = IsAdmin() ? ativo?.EmpresaId : GetEmpresaId();
            var cats = empId.HasValue
                ? db.Categorias.Where(c => c.EmpresaId == empId).ToList()
                : db.Categorias.ToList();
            var cols = empId.HasValue
                ? db.Colaboradores.Where(c => c.EmpresaId == empId).ToList()
                : db.Colaboradores.ToList();

            ViewBag.Categorias  = new SelectList(cats, "Id", "Nome", ativo?.CategoriaId);
            ViewBag.Colaboradores = new SelectList(cols, "Id", "Nome", ativo?.ColaboradorId);
            ViewBag.EmpresaSelectList = IsAdmin()
                ? new SelectList(db.Empresas, "Id", "Nome", ativo?.EmpresaId)
                : new SelectList(db.Empresas.Where(e => e.Id == empId).ToList(), "Id", "Nome", empId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
