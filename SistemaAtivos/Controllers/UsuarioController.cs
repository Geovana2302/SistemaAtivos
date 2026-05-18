using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class UsuarioController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int GetUsuarioLogadoId() => (int)Session["UsuarioId"];
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

        private IQueryable<Usuario> GetQuery()
        {
            var q = db.Usuarios.Include(u => u.Empresa).AsQueryable();
            if (!IsAdmin())
            {
                var empresaId = GetEmpresaId();
                q = q.Where(u => u.EmpresaId == empresaId);
            }
            return q;
        }

        public ActionResult Index(int? empresaId)
        {
            var q = GetQuery();
            if (IsAdmin() && empresaId.HasValue)
                q = q.Where(u => u.EmpresaId == empresaId);

            ViewBag.Empresas      = db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;
            return View(q.OrderBy(u => u.Nome).ToList());
        }

        [HttpGet]
        public ActionResult Create(int? empresaId)
        {
            if (!IsAdmin() && empresaId.HasValue && empresaId != GetEmpresaId())
                return new HttpUnauthorizedResult();

            var usuario = new Usuario { Perfil = Perfil.Gestor };
            if (empresaId.HasValue)
                usuario.EmpresaId = empresaId;
            else if (!IsAdmin())
                usuario.EmpresaId = GetEmpresaId();

            PopularDropdowns(usuario);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuario usuario, string senha)
        {
            if (!IsAdmin())
            {
                usuario.EmpresaId = GetEmpresaId();
                usuario.Perfil    = Perfil.Gestor;
            }

            if (string.IsNullOrWhiteSpace(senha))
                ModelState.AddModelError("senha", "A senha é obrigatória.");

            if (db.Usuarios.Any(u => u.Email == usuario.Email))
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");

            ModelState.Remove("Senha");

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(senha);
                    db.Usuarios.Add(usuario);
                    db.SaveChanges();
                    TempData["Sucesso"] = "Usuário cadastrado com sucesso.";
                    if (usuario.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Erro"] = "Erro ao cadastrar usuário. Tente novamente.";
                }
            }

            PopularDropdowns(usuario);
            return View(usuario);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            Usuario usuario = GetQuery().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return HttpNotFound();
            PopularDropdowns(usuario);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string Nome, string Email, string senha, Perfil Perfil, int? EmpresaId, string senhaAdmin)
        {
            var usuario = GetQuery().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return HttpNotFound();

            var usuarioLogado = db.Usuarios.Find(GetUsuarioLogadoId());
            if (usuarioLogado == null || !BCrypt.Net.BCrypt.Verify(senhaAdmin, usuarioLogado.Senha))
                ModelState.AddModelError("senhaAdmin", "Senha incorreta.");

            if (db.Usuarios.Any(u => u.Email == Email && u.Id != id))
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado por outro usuário.");

            if (!ModelState.IsValid)
            {
                PopularDropdowns(usuario);
                return View(usuario);
            }

            try
            {
                usuario.Nome  = Nome;
                usuario.Email = Email;

                if (IsAdmin())
                {
                    usuario.Perfil    = Perfil;
                    usuario.EmpresaId = EmpresaId;
                }

                if (!string.IsNullOrWhiteSpace(senha))
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(senha);

                db.SaveChanges();
                TempData["Sucesso"] = "Usuário atualizado com sucesso.";
                if (usuario.EmpresaId.HasValue)
                    return RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
                return RedirectToAction("Admins", "Admin");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao atualizar usuário. Tente novamente.";
                PopularDropdowns(usuario);
                return View(usuario);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, string senhaConfirm)
        {
            if (id == GetUsuarioLogadoId())
            {
                TempData["Erro"] = "Você não pode excluir seu próprio usuário.";
                return RedirectToAction("Empresa", "Admin", new { id = GetEmpresaId() });
            }

            // Valida senha de quem está excluindo
            var logado = db.Usuarios.Find(GetUsuarioLogadoId());
            if (logado == null || !BCrypt.Net.BCrypt.Verify(senhaConfirm, logado.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Exclusão cancelada.";
                var empIdFail = GetEmpresaId();
                if (empIdFail.HasValue)
                    return RedirectToAction("Empresa", "Admin", new { id = empIdFail });
                return RedirectToAction("Index");
            }

            var usuario = GetQuery().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return HttpNotFound();
            var empId = usuario.EmpresaId;

            try
            {
                db.Usuarios.Remove(usuario);
                db.SaveChanges();
                TempData["Sucesso"] = "Gestor excluído com sucesso.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir gestor. Tente novamente.";
            }

            if (empId.HasValue)
                return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }

        private void PopularDropdowns(Usuario usuario)
        {
            ViewBag.EmpresaSelectList = IsAdmin()
                ? new SelectList(db.Empresas, "Id", "Nome", usuario?.EmpresaId)
                : new SelectList(db.Empresas.Where(e => e.Id == GetEmpresaId()), "Id", "Nome", GetEmpresaId());

            ViewBag.Perfis = new SelectList(
                new[] { Perfil.Admin, Perfil.Gestor }.Select(p => new { Value = p.ToString(), Text = p == Perfil.Admin ? "Administrador" : "Gestor de Empresa" }),
                "Value", "Text", usuario?.Perfil.ToString());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
