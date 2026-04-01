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
                q = q.Where(a => a.EmpresaId == GetEmpresaId());
            return q;
        }

        public ActionResult Index(int? empresaId, string ordem)
        {
            var q = GetQuery().Where(a => a.Status == StatusAtivo.Ativo);

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

        public ActionResult Detalhes(int id)
        {
            var ativo = GetQuery()
                .Include(a => a.Manutencoes)
                .FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();
            return View(ativo);
        }

        public ActionResult QrCode(int id)
        {
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();

            var url = Url.Action("Detalhes", "Ativo", new { id }, Request.Url.Scheme);
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
            var ativo = GetQuery().FirstOrDefault(a => a.Id == id);
            if (ativo == null) return HttpNotFound();
            var empId = ativo.EmpresaId;

            try
            {
                var mans = db.Manutencoes.Where(m => m.AtivoId == id).ToList();
                db.Manutencoes.RemoveRange(mans);
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
            var q = GetQuery().Where(a => a.Status == StatusAtivo.Inativo);

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

        private void PopularDropdowns(Ativo ativo = null)
        {
            var empId = IsAdmin() ? ativo?.EmpresaId : GetEmpresaId();
            var cats = empId.HasValue
                ? db.Categorias.Where(c => c.EmpresaId == empId).ToList()
                : db.Categorias.ToList();
            var cols = empId.HasValue
                ? db.Colaboradores.Where(c => c.EmpresaId == empId).ToList()
                : db.Colaboradores.ToList();

            ViewBag.CategoriaId  = new SelectList(cats, "Id", "Nome", ativo?.CategoriaId);
            ViewBag.ColaboradorId = new SelectList(cols, "Id", "Nome", ativo?.ColaboradorId);
            ViewBag.EmpresaId    = IsAdmin()
                ? new SelectList(db.Empresas, "Id", "Nome", ativo?.EmpresaId)
                : new SelectList(db.Empresas.Where(e => e.Id == GetEmpresaId()), "Id", "Nome", GetEmpresaId());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
