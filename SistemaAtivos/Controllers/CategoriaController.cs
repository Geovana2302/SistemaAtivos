using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class CategoriaController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

        private IQueryable<Categoria> GetQuery()
        {
            var q = db.Categorias.Include(c => c.Empresa).AsQueryable();
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
            var cat = new Categoria();
            if (empresaId.HasValue)
                cat.EmpresaId = empresaId;
            else if (!IsAdmin())
                cat.EmpresaId = GetEmpresaId();

            ViewBag.EmpresaId = IsAdmin()
                ? new SelectList(db.Empresas, "Id", "Nome", cat.EmpresaId)
                : new SelectList(db.Empresas.Where(e => e.Id == GetEmpresaId()), "Id", "Nome", GetEmpresaId());
            ViewBag.EmpresaPreSelecionada = empresaId.HasValue || !IsAdmin();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Categoria categoria)
        {
            if (!IsAdmin()) categoria.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Categorias.Add(categoria);
                    db.SaveChanges();
                    TempData["Sucesso"] = "Categoria criada com sucesso.";
                    if (categoria.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = categoria.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao criar categoria. Tente novamente.";
                }
            }
            ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", categoria.EmpresaId);
            return View(categoria);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var cat = GetQuery().FirstOrDefault(c => c.Id == id);
            if (cat == null) return HttpNotFound();
            ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", cat.EmpresaId);
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Categoria categoria)
        {
            if (!IsAdmin()) categoria.EmpresaId = GetEmpresaId();
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(categoria).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["Sucesso"] = "Categoria atualizada.";
                    if (categoria.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = categoria.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao atualizar categoria. Tente novamente.";
                }
            }
            ViewBag.EmpresaId = new SelectList(db.Empresas, "Id", "Nome", categoria.EmpresaId);
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var cat = GetQuery().FirstOrDefault(c => c.Id == id);
            if (cat == null) return HttpNotFound();
            var empId = cat.EmpresaId;

            if (db.Ativos.Any(a => a.CategoriaId == id))
            {
                TempData["Erro"] = "Não é possível excluir esta categoria pois existem ativos vinculados a ela.";
                if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
                return RedirectToAction("Index");
            }

            try
            {
                db.Categorias.Remove(cat);
                db.SaveChanges();
                TempData["Sucesso"] = "Categoria excluída.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir categoria. Tente novamente.";
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
