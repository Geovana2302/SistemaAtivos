using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class ColaboradorController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

        private IQueryable<Colaborador> GetQuery()
        {
            var q = db.Colaboradores.Include(c => c.Empresa).AsQueryable();
            if (!IsAdmin())
                q = q.Where(c => c.EmpresaId == GetEmpresaId());
            return q;
        }

        public ActionResult Index()
        {
            return View(GetQuery().ToList());
        }

        [HttpGet]
        public ActionResult Create(int? empresaId)
        {
            if (empresaId.HasValue)
            {
                ViewBag.EmpresaId = IsAdmin()
                    ? new SelectList(db.Empresas, "Id", "Nome", empresaId)
                    : new SelectList(db.Empresas.Where(e => e.Id == GetEmpresaId()), "Id", "Nome", empresaId);
                ViewBag.EmpresaPreSelecionada = true;
                var col = new Colaborador { EmpresaId = empresaId };
                return View(col);
            }
            ViewBag.EmpresaId = IsAdmin()
                ? new SelectList(db.Empresas, "Id", "Nome")
                : new SelectList(db.Empresas.Where(e => e.Id == GetEmpresaId()), "Id", "Nome");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Colaborador colaborador)
        {
            if (!IsAdmin()) colaborador.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Colaboradores.Add(colaborador);
                    db.SaveChanges();
                    TempData["Sucesso"] = "Colaborador cadastrado.";
                    if (colaborador.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = colaborador.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao cadastrar colaborador. Tente novamente.";
                }
            }
            ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", colaborador.EmpresaId);
            return View(colaborador);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var col = GetQuery().FirstOrDefault(c => c.Id == id);
            if (col == null) return HttpNotFound();
            ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", col.EmpresaId);
            return View(col);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Colaborador colaborador)
        {
            if (!IsAdmin()) colaborador.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(colaborador).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Sucesso"] = "Colaborador atualizado.";
                    if (colaborador.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = colaborador.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao atualizar colaborador. Tente novamente.";
                }
            }
            ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", colaborador.EmpresaId);
            return View(colaborador);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var col = GetQuery().FirstOrDefault(c => c.Id == id);
            if (col == null) return HttpNotFound();
            var empId = col.EmpresaId;

            if (db.Ativos.Any(a => a.ColaboradorId == id))
            {
                TempData["Erro"] = "Não é possível excluir este colaborador pois existem ativos vinculados a ele.";
                if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
                return RedirectToAction("Index");
            }

            try
            {
                db.Colaboradores.Remove(col);
                db.SaveChanges();
                TempData["Sucesso"] = "Colaborador excluído.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir colaborador. Tente novamente.";
            }
            if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
