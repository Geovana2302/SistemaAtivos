using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class ColaboradorController : Controller
    {
        private readonly AtivosContext _db;

        public ColaboradorController(AtivosContext db) => _db = db;

        private bool IsAdmin() => HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");

        private IQueryable<Colaborador> GetQuery()
        {
            var q = _db.Colaboradores.Include(c => c.Empresa).AsQueryable();
            if (!IsAdmin()) q = q.Where(c => c.EmpresaId == GetEmpresaId());
            return q;
        }

        public IActionResult Index() => View(GetQuery().ToList());

        [HttpGet]
        public IActionResult Create(int? empresaId)
        {
            var col = new Colaborador();
            if (empresaId.HasValue) col.EmpresaId = empresaId;
            else if (!IsAdmin()) col.EmpresaId = GetEmpresaId();

            ViewBag.Empresas = IsAdmin()
                ? new SelectList(_db.Empresas, "Id", "Nome", col.EmpresaId)
                : new SelectList(_db.Empresas.Where(e => e.Id == GetEmpresaId()).ToList(), "Id", "Nome", GetEmpresaId());
            ViewBag.EmpresaPreSelecionada = empresaId.HasValue || !IsAdmin();
            return View(col);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Colaborador colaborador)
        {
            if (!IsAdmin()) colaborador.EmpresaId = GetEmpresaId();

            if (!colaborador.EmpresaId.HasValue)
                ModelState.AddModelError("EmpresaId", "Empresa é obrigatória.");

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Colaboradores.Add(colaborador);
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Colaborador cadastrado.";
                    if (colaborador.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = colaborador.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao cadastrar colaborador."; }
            }

            ViewBag.Empresas = new SelectList(_db.Empresas, "Id", "Nome", colaborador.EmpresaId);
            return View(colaborador);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var col = GetQuery().FirstOrDefault(c => c.Id == id);
            if (col == null) return NotFound();
            ViewBag.Empresas = new SelectList(_db.Empresas, "Id", "Nome", col.EmpresaId);
            return View(col);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Colaborador colaborador)
        {
            if (!IsAdmin()) colaborador.EmpresaId = GetEmpresaId();

            if (!colaborador.EmpresaId.HasValue)
                ModelState.AddModelError("EmpresaId", "Empresa é obrigatória.");

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Entry(colaborador).State = EntityState.Modified;
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Colaborador atualizado.";
                    if (colaborador.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = colaborador.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao atualizar colaborador."; }
            }

            ViewBag.Empresas = new SelectList(_db.Empresas, "Id", "Nome", colaborador.EmpresaId);
            return View(colaborador);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var col = GetQuery().FirstOrDefault(c => c.Id == id);
            if (col == null) return NotFound();
            var empId = col.EmpresaId;

            if (_db.Ativos.Any(a => a.ColaboradorId == id))
            {
                TempData["Erro"] = "Não é possível excluir este colaborador pois existem ativos vinculados.";
                if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
                return RedirectToAction("Index");
            }

            try
            {
                _db.Colaboradores.Remove(col);
                _db.SaveChanges();
                TempData["Sucesso"] = "Colaborador excluído.";
            }
            catch { TempData["Erro"] = "Erro ao excluir colaborador."; }

            if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }
    }
}
