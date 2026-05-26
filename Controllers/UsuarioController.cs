using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    [EmpresaAuthorize]
    public class UsuarioController : Controller
    {
        private readonly AtivosContext _db;

        public UsuarioController(AtivosContext db) => _db = db;

        private bool IsAdmin() => HttpContext.Session.GetString("UsuarioTipo") == "Admin";
        private int GetUsuarioLogadoId() => HttpContext.Session.GetInt32("UsuarioId")!.Value;
        private int? GetEmpresaId() => HttpContext.Session.GetInt32("EmpresaId");

        private IQueryable<Usuario> GetQuery()
        {
            var q = _db.Usuarios.Include(u => u.Empresa).AsQueryable();
            if (!IsAdmin()) q = q.Where(u => u.EmpresaId == GetEmpresaId());
            return q;
        }

        public IActionResult Index(int? empresaId)
        {
            var q = GetQuery();
            if (IsAdmin() && empresaId.HasValue) q = q.Where(u => u.EmpresaId == empresaId);
            ViewBag.Empresas      = _db.Empresas.ToList();
            ViewBag.EmpresaFiltro = empresaId;
            return View(q.OrderBy(u => u.Nome).ToList());
        }

        [HttpGet]
        public IActionResult Create(int? empresaId)
        {
            if (!IsAdmin() && empresaId.HasValue && empresaId != GetEmpresaId()) return Forbid();

            var usuario = new Usuario { Perfil = Perfil.Gestor };
            if (empresaId.HasValue) usuario.EmpresaId = empresaId;
            else if (!IsAdmin()) usuario.EmpresaId = GetEmpresaId();

            PopularDropdowns(usuario);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario, string senha)
        {
            if (!IsAdmin())
            {
                usuario.EmpresaId = GetEmpresaId();
                usuario.Perfil    = Perfil.Gestor;
            }

            if (string.IsNullOrWhiteSpace(senha))
                ModelState.AddModelError("senha", "A senha é obrigatória.");

            if (_db.Usuarios.Any(u => u.Email == usuario.Email))
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");

            ModelState.Remove("Senha");

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(senha);
                    _db.Usuarios.Add(usuario);
                    _db.SaveChanges();
                    TempData["Sucesso"] = "Usuário cadastrado com sucesso.";
                    if (usuario.EmpresaId.HasValue)
                        return RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
                    return RedirectToAction("Index");
                }
                catch { TempData["Erro"] = "Erro ao cadastrar usuário. Tente novamente."; }
            }

            PopularDropdowns(usuario);
            return View(usuario);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var usuario = GetQuery().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return NotFound();
            PopularDropdowns(usuario);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string Nome, string Email, string? senha, Perfil Perfil, int? EmpresaId, string senhaAdmin)
        {
            var usuario = GetQuery().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return NotFound();

            var usuarioLogado = _db.Usuarios.Find(GetUsuarioLogadoId());
            if (usuarioLogado == null || !BCrypt.Net.BCrypt.Verify(senhaAdmin, usuarioLogado.Senha))
                ModelState.AddModelError("senhaAdmin", "Senha incorreta.");

            if (_db.Usuarios.Any(u => u.Email == Email && u.Id != id))
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

                _db.SaveChanges();
                TempData["Sucesso"] = "Usuário atualizado com sucesso.";
                if (usuario.EmpresaId.HasValue)
                    return RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
                return RedirectToAction("Admins", "Admin");
            }
            catch
            {
                TempData["Erro"] = "Erro ao atualizar usuário.";
                PopularDropdowns(usuario);
                return View(usuario);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string senhaConfirm)
        {
            if (id == GetUsuarioLogadoId())
            {
                TempData["Erro"] = "Você não pode excluir seu próprio usuário.";
                return RedirectToAction("Empresa", "Admin", new { id = GetEmpresaId() });
            }

            var logado = _db.Usuarios.Find(GetUsuarioLogadoId());
            if (logado == null || !BCrypt.Net.BCrypt.Verify(senhaConfirm, logado.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Exclusão cancelada.";
                var empIdFail = GetEmpresaId();
                if (empIdFail.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empIdFail });
                return RedirectToAction("Index");
            }

            var usuario = GetQuery().FirstOrDefault(u => u.Id == id);
            if (usuario == null) return NotFound();
            var empId = usuario.EmpresaId;

            try
            {
                _db.Usuarios.Remove(usuario);
                _db.SaveChanges();
                TempData["Sucesso"] = "Gestor excluído com sucesso.";
            }
            catch { TempData["Erro"] = "Erro ao excluir gestor."; }

            if (empId.HasValue) return RedirectToAction("Empresa", "Admin", new { id = empId });
            return RedirectToAction("Index");
        }

        private void PopularDropdowns(Usuario? usuario)
        {
            ViewBag.EmpresaSelectList = IsAdmin()
                ? new SelectList(_db.Empresas, "Id", "Nome", usuario?.EmpresaId)
                : new SelectList(_db.Empresas.Where(e => e.Id == GetEmpresaId()).ToList(), "Id", "Nome", GetEmpresaId());

            ViewBag.Perfis = new SelectList(
                new[]
                {
                    new { Value = "Admin",  Text = "Administrador" },
                    new { Value = "Gestor", Text = "Gestor de Empresa" }
                }, "Value", "Text", usuario?.Perfil.ToString());
        }
    }
}
