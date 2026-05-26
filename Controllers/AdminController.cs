using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class AdminController : Controller
    {
        private readonly AtivosContext _db;

        public AdminController(AtivosContext db) => _db = db;

        private bool IsAdmin() => HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");
        private int GetUsuarioId() => HttpContext.Session.GetInt32("UsuarioId")!.Value;

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var empresas = _db.Empresas
                .Include(e => e.Ativos)
                .Include(e => e.Categorias)
                .Include(e => e.Colaboradores)
                .Include(e => e.Usuarios)
                .ToList();

            return View(empresas);
        }

        // ── Empresa ──────────────────────────────────────────────────────

        public IActionResult Empresa(int id)
        {
            if (!IsAdmin() && GetEmpresaId() != id) return Forbid();

            var empresa = _db.Empresas
                .Include(e => e.Ativos).ThenInclude(a => a.Categoria)
                .Include(e => e.Ativos).ThenInclude(a => a.Colaborador)
                .Include(e => e.Categorias)
                .Include(e => e.Colaboradores)
                .Include(e => e.Usuarios)
                .FirstOrDefault(e => e.Id == id);

            if (empresa == null) return NotFound();
            return View(empresa);
        }

        [HttpGet]
        public IActionResult CreateEmpresa()
        {
            if (!IsAdmin()) return Forbid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateEmpresa(Empresa empresa)
        {
            if (!IsAdmin()) return Forbid();

            if (_db.Empresas.Count() >= 25)
            {
                TempData["Erro"] = "Limite de 25 empresas atingido.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(empresa.Nome))
            {
                TempData["Erro"] = "Nome da empresa é obrigatório.";
                return View(empresa);
            }

            if (string.IsNullOrWhiteSpace(empresa.Cor)) empresa.Cor = "#534AB7";

            try
            {
                _db.Empresas.Add(empresa);
                _db.SaveChanges();
                TempData["Sucesso"] = $"Empresa \"{empresa.Nome}\" criada com sucesso!";
            }
            catch { TempData["Erro"] = "Erro ao criar empresa. Tente novamente."; }

            return RedirectToAction("Empresa", new { id = empresa.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEmpresa(int Id, string Nome, string Cor, string senhaAdmin)
        {
            if (!IsAdmin()) return Forbid();

            var admin = _db.Usuarios.Find(GetUsuarioId());
            if (admin == null || !BCrypt.Net.BCrypt.Verify(senhaAdmin, admin.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Edição cancelada.";
                return RedirectToAction("Dashboard");
            }

            var empresa = _db.Empresas.Find(Id);
            if (empresa == null) return NotFound();

            if (string.IsNullOrWhiteSpace(Nome))
            {
                TempData["Erro"] = "Nome da empresa é obrigatório.";
                return RedirectToAction("Dashboard");
            }

            empresa.Nome = Nome;
            empresa.Cor  = string.IsNullOrWhiteSpace(Cor) ? "#534AB7" : Cor;

            try { _db.SaveChanges(); TempData["Sucesso"] = "Empresa atualizada."; }
            catch { TempData["Erro"] = "Erro ao atualizar empresa."; }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEmpresa(int id, string senhaAdmin)
        {
            if (!IsAdmin()) return Forbid();

            var admin = _db.Usuarios.Find(GetUsuarioId());
            if (admin == null || !BCrypt.Net.BCrypt.Verify(senhaAdmin, admin.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Exclusão cancelada.";
                return RedirectToAction("Dashboard");
            }

            var empresa = _db.Empresas.Find(id);
            if (empresa == null) return NotFound();

            try
            {
                _db.Ativos.RemoveRange(_db.Ativos.Where(a => a.EmpresaId == id));
                _db.Categorias.RemoveRange(_db.Categorias.Where(c => c.EmpresaId == id));
                _db.Colaboradores.RemoveRange(_db.Colaboradores.Where(c => c.EmpresaId == id));
                _db.Usuarios.RemoveRange(_db.Usuarios.Where(u => u.EmpresaId == id));
                _db.Empresas.Remove(empresa);
                _db.SaveChanges();
                TempData["Sucesso"] = $"Empresa \"{empresa.Nome}\" excluída com sucesso.";
            }
            catch { TempData["Erro"] = "Erro ao excluir empresa."; }

            return RedirectToAction("Dashboard");
        }

        // ── Admins ───────────────────────────────────────────────────────

        public IActionResult Admins()
        {
            if (!IsAdmin()) return Forbid();
            var admins = _db.Usuarios.Where(u => u.Perfil == Perfil.Admin).OrderBy(u => u.Nome).ToList();
            return View(admins);
        }

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            if (!IsAdmin()) return Forbid();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateAdmin(string Nome, string Email, string senha)
        {
            if (!IsAdmin()) return Forbid();

            if (string.IsNullOrWhiteSpace(senha))
                ModelState.AddModelError("senha", "A senha é obrigatória.");

            if (_db.Usuarios.Any(u => u.Email == Email))
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");

            if (!ModelState.IsValid) return View();

            try
            {
                _db.Usuarios.Add(new Usuario
                {
                    Nome   = Nome,
                    Email  = Email,
                    Senha  = BCrypt.Net.BCrypt.HashPassword(senha),
                    Perfil = Perfil.Admin
                });
                _db.SaveChanges();
                TempData["Sucesso"] = "Administrador cadastrado com sucesso.";
            }
            catch { TempData["Erro"] = "Erro ao cadastrar administrador."; }

            return RedirectToAction("Admins");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAdmin(int id, string senhaAdmin)
        {
            if (!IsAdmin()) return Forbid();

            var adminLogado = _db.Usuarios.Find(GetUsuarioId());
            if (adminLogado == null || !adminLogado.IsSuperAdmin)
            {
                TempData["Erro"] = "Apenas o Super Admin pode excluir administradores.";
                return RedirectToAction("Admins");
            }

            if (!BCrypt.Net.BCrypt.Verify(senhaAdmin, adminLogado.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Exclusão cancelada.";
                return RedirectToAction("Admins");
            }

            var usuario = _db.Usuarios.Find(id);
            if (usuario == null || usuario.Perfil != Perfil.Admin || usuario.IsSuperAdmin)
                return NotFound();

            try
            {
                _db.Usuarios.Remove(usuario);
                _db.SaveChanges();
                TempData["Sucesso"] = "Administrador excluído.";
            }
            catch { TempData["Erro"] = "Erro ao excluir administrador."; }

            return RedirectToAction("Admins");
        }
    }
}
