using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    public class AccountController : Controller
    {
        private AtivosContext db = new AtivosContext();

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["UsuarioId"] != null)
            {
                var tipo = Session["UsuarioTipo"]?.ToString();
                if (tipo == "Admin")
                    return RedirectToAction("Dashboard", "Admin");
                else
                    return RedirectToAction("Empresa", "Admin", new { id = Session["EmpresaId"] });
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string senha)
        {
            var usuario = db.Usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(senha, usuario.Senha))
            {
                ViewBag.Erro = "E-mail ou senha incorretos.";
                return View();
            }

            Session["UsuarioId"]   = usuario.Id;
            Session["UsuarioNome"] = usuario.Nome;
            Session["UsuarioTipo"] = usuario.Tipo.ToString();
            Session["EmpresaId"]   = usuario.EmpresaId;

            if (usuario.Tipo == TipoUsuario.Admin)
                return RedirectToAction("Dashboard", "Admin");
            else
                return RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
