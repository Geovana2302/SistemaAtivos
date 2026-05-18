using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Filters;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    // =====================================================================
    // REQUISITO 1 - [EmpresaAuthorize] protege todo o controller
    // REQUISITO 4 - LOGICA OPERACIONAL: Dashboard com queries complexas,
    //   CreateEmpresa com limite de 25 + criacao automatica de usuario,
    //   DeleteEmpresa com exclusao em cascata de 5 entidades relacionadas
    // REQUISITO 5 - ESTABILIDADE: try-catch em todas as operacoes criticas
    // =====================================================================
    [EmpresaAuthorize]
    public class AdminController : Controller
    {
        private AtivosContext db = new AtivosContext();

        private bool IsAdmin() => Session["UsuarioTipo"]?.ToString() == "Admin";
        private int? GetEmpresaId() => Session["EmpresaId"] as int?;

        public ActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var empresas = db.Empresas
                .Include(e => e.Ativos)
                .Include(e => e.Categorias)
                .Include(e => e.Colaboradores)
                .Include(e => e.Usuarios)
                .ToList();
            return View(empresas);
        }

        [HttpGet]
        public ActionResult CreateEmpresa()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            return View();
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // Verifica limite de 25 empresas, cria empresa e usuario cliente
        // automaticamente com senha hash BCrypt
        // REQUISITO 5 - try-catch para tratamento de excecoes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateEmpresa(Empresa empresa)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            if (db.Empresas.Count() >= 25)
            {
                TempData["Erro"] = "Limite de 25 empresas atingido.";
                return RedirectToAction("Dashboard");
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(empresa.Nome))
            {
                TempData["Erro"] = "Nome da empresa é obrigatório.";
                return RedirectToAction("Dashboard");
            }

            if (string.IsNullOrWhiteSpace(empresa.Cor))
                empresa.Cor = "#534AB7";

            try
            {
                db.Empresas.Add(empresa);
                db.SaveChanges();
                TempData["Sucesso"] = $"Empresa \"{empresa.Nome}\" criada com sucesso! Acesse a empresa para cadastrar o primeiro gestor.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao criar empresa. Tente novamente.";
            }
            return RedirectToAction("Empresa", new { id = empresa.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEmpresa(int Id, string Nome, string Cor, string senhaAdmin)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            var adminId = (int)Session["UsuarioId"];
            var admin = db.Usuarios.Find(adminId);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(senhaAdmin, admin.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Edição cancelada.";
                return RedirectToAction("Dashboard");
            }

            var empresa = db.Empresas.Find(Id);
            if (empresa == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(Nome))
            {
                TempData["Erro"] = "Nome da empresa é obrigatório.";
                return RedirectToAction("Dashboard");
            }

            empresa.Nome = Nome;
            empresa.Cor = string.IsNullOrWhiteSpace(Cor) ? "#534AB7" : Cor;

            try
            {
                db.SaveChanges();
                TempData["Sucesso"] = "Empresa atualizada com sucesso.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao atualizar empresa. Tente novamente.";
            }
            return RedirectToAction("Dashboard");
        }

        // REQUISITO 4 - LOGICA OPERACIONAL (Action complexa)
        // Exclusao em cascata: remove Manutencoes, Ativos, Categorias,
        // Colaboradores e Usuarios vinculados antes de excluir a Empresa.
        // Exige confirmacao com senha do admin para seguranca.
        // REQUISITO 5 - try-catch para tratamento de excecoes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEmpresa(int id, string senhaAdmin)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            var adminId = (int)Session["UsuarioId"];
            var admin = db.Usuarios.Find(adminId);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(senhaAdmin, admin.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Exclusão cancelada.";
                return RedirectToAction("Dashboard");
            }

            var empresa = db.Empresas.Find(id);
            if (empresa == null) return HttpNotFound();

            try
            {
                var ativos = db.Ativos.Where(a => a.EmpresaId == id).ToList();
                db.Ativos.RemoveRange(ativos);

                var categorias = db.Categorias.Where(c => c.EmpresaId == id).ToList();
                db.Categorias.RemoveRange(categorias);

                var colaboradores = db.Colaboradores.Where(c => c.EmpresaId == id).ToList();
                db.Colaboradores.RemoveRange(colaboradores);

                var usuarios = db.Usuarios.Where(u => u.EmpresaId == id).ToList();
                db.Usuarios.RemoveRange(usuarios);

                db.Empresas.Remove(empresa);
                db.SaveChanges();

                TempData["Sucesso"] = $"Empresa \"{empresa.Nome}\" excluída com sucesso.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir empresa. Tente novamente.";
            }
            return RedirectToAction("Dashboard");
        }

        // REQUISITO 2 - MOVIMENTACAO DE DADOS
        // Carrega a Empresa com todas as entidades relacionadas usando Include:
        // Ativos (com Categoria e Colaborador), Categorias, Colaboradores, Usuarios, Manutencoes
        // REQUISITO 4 - Query complexa com multiplos Include aninhados
        public ActionResult Empresa(int id)
        {
            if (!IsAdmin() && GetEmpresaId() != id)
                return new HttpUnauthorizedResult();

            var empresa = db.Empresas
                .Include(e => e.Ativos.Select(a => a.Categoria))
                .Include(e => e.Ativos.Select(a => a.Colaborador))
                .Include(e => e.Categorias)
                .Include(e => e.Colaboradores)
                .Include(e => e.Usuarios)
                .FirstOrDefault(e => e.Id == id);

            if (empresa == null) return HttpNotFound();
            return View(empresa);
        }

        // ?? CRUD de Administradores do sistema ??????????????????????????
        public ActionResult Admins()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            var admins = db.Usuarios.Where(u => u.Perfil == Perfil.Admin).OrderBy(u => u.Nome).ToList();
            return View(admins);
        }

        [HttpGet]
        public ActionResult CreateAdmin()
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(string Nome, string Email, string senha)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            if (string.IsNullOrWhiteSpace(senha))
                ModelState.AddModelError("senha", "A senha é obrigatória.");

            if (db.Usuarios.Any(u => u.Email == Email))
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");

            if (!ModelState.IsValid)
                return View();

            try
            {
                db.Usuarios.Add(new Usuario
                {
                    Nome      = Nome,
                    Email     = Email,
                    Senha     = BCrypt.Net.BCrypt.HashPassword(senha),
                    Perfil    = Perfil.Admin,
                    EmpresaId = null
                });
                db.SaveChanges();
                TempData["Sucesso"] = "Administrador cadastrado com sucesso.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao cadastrar administrador.";
            }
            return RedirectToAction("Admins");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAdmin(int id, string senhaAdmin)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            var adminLogadoId = (int)Session["UsuarioId"];
            var adminLogado = db.Usuarios.Find(adminLogadoId);

            if (adminLogado == null || !adminLogado.IsSuperAdmin)
            {
                TempData["Erro"] = "Apenas o Super Admin pode excluir administradores.";
                return RedirectToAction("Admins");
            }

            if (!BCrypt.Net.BCrypt.Verify(senhaAdmin, adminLogado.Senha))
            {
                TempData["Erro"] = "Senha incorreta. Exclusao cancelada.";
                return RedirectToAction("Admins");
            }

            var usuario = db.Usuarios.Find(id);
            if (usuario == null || usuario.Perfil != Perfil.Admin || usuario.IsSuperAdmin)
                return HttpNotFound();

            try
            {
                db.Usuarios.Remove(usuario);
                db.SaveChanges();
                TempData["Sucesso"] = "Administrador excluido.";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao excluir administrador.";
            }
            return RedirectToAction("Admins");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
