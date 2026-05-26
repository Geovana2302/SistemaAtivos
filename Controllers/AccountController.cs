using Microsoft.AspNetCore.Mvc;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    public class AccountController : Controller
    {
        private readonly AtivosContext _db;

        public AccountController(AtivosContext db) => _db = db;

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UsuarioId") != null)
            {
                var tipo = HttpContext.Session.GetString("UsuarioTipo");
                return tipo == "Admin"
                    ? RedirectToAction("Dashboard", "Admin")
                    : RedirectToAction("Empresa", "Admin", new { id = HttpContext.Session.GetInt32("EmpresaId") });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string senha)
        {
            var usuario = _db.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
            {
                ViewBag.Erro = "E-mail ou senha incorretos.";
                return View();
            }

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
            HttpContext.Session.SetString("UsuarioTipo", usuario.Perfil.ToString());
            HttpContext.Session.SetString("IsSuperAdmin", usuario.IsSuperAdmin ? "true" : "false");
            if (usuario.EmpresaId.HasValue)
                HttpContext.Session.SetInt32("EmpresaId", usuario.EmpresaId.Value);

            return usuario.Perfil == Perfil.Admin
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
