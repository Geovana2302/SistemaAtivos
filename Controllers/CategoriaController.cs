using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class CategoriaController : Controller
    {
        private readonly AtivosContext _db;

        public CategoriaController(AtivosContext db) => _db = db;

        private bool IsAdmin() => HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");

        private IQueryable<Categoria> GetQuery()
        {
            var q = _db.Categorias.Include(c => c.Empresa).AsQueryable();
            if (!IsAdmin()) q = q.Where(c => c.EmpresaId == GetEmpresaId());
            return q;
        }

        public IActionResult Index(int? empresaId)
        {
            var q = GetQuery().Include(c => c.Ativos).AsQueryable();
            if (IsAdmin() && empresaId.HasValue) q = q.Where(c => c.EmpresaId == empresaId);
            ViewBag.Empresas      = _db.Empresas.OrderBy(e => e.Nome).ToList();
            ViewBag.EmpresaFiltro = empresaId;
            return View(q.ToList());
        }

        [HttpGet]
        public IActionResult Create(int? empresaId)
        {
            var cat = new Categoria();
            if (empresaId.HasValue) cat.EmpresaId = empresaId;
            else if (!IsAdmin()) cat.EmpresaId = GetEmpresaId();

            ViewBag.Empresas = IsAdmin()
                ? new SelectList(_db.Empresas, "Id", "Nome", cat.EmpresaId)
                : new SelectList(_db.Empresas.Where(e => e.Id == GetEmpresaId()).ToList(), "Id", "Nome", GetEmpresaId());
            ViewBag.EmpresaPreSelecionada = empresaId.HasValue || !IsAdmin();
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria categoria)
        {
            if (!IsAdmin()) categoria.EmpresaId = GetEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Categorias.Add(categoria);
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Categoria criada com sucesso.";
                    if (categoria.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = categoria.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao criar categoria. Tente novamente."; }
            }

            ViewBag.Empresas = new SelectList(_db.Empresas, "Id", "Nome", categoria.EmpresaId);
            return View(categoria);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var cat = GetQuery().FirstOrDefault(c => c.Id == id);
            if (cat == null) return NotFound();
            ViewBag.Empresas = new SelectList(_db.Empresas, "Id", "Nome", cat.EmpresaId);
            return View(cat);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Categoria categoria)
        {
            if (!IsAdmin()) categoria.EmpresaId = GetEmpresaId();

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Entry(categoria).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Categoria atualizada.";
                    if (categoria.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = categoria.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao atualizar categoria."; }
            }

            ViewBag.Empresas = new SelectList(_db.Empresas, "Id", "Nome", categoria.EmpresaId);
            return View(categoria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var cat = GetQuery().FirstOrDefault(c => c.Id == id);
            if (cat == null) return NotFound();
            var empId = cat.EmpresaId;

            if (_db.Ativos.Any(a => a.CategoriaId == id))
            {
                TempData["Erro"] = "Não é possível excluir esta categoria pois existem ativos vinculados.";
                if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
                return RedirectToAction("Index");
            }

            try
            {
                _db.Categorias.Remove(cat);
                _db.SaveChanges();
                TempData["Sucesso"] = "Categoria excluída.";
            }
            catch { TempData["Erro"] = "Erro ao excluir categoria."; }

            if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }
    }
}
