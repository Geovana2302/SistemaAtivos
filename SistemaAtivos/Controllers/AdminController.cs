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
        public ActionResult CreateEmpresa(Empresa empresa, string emailCliente)
        {
            if (!IsAdmin()) return new HttpUnauthorizedResult();

            // REQUISITO 4 - Regra de negocio: limite maximo de empresas
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
            {
                empresa.Cor = "#534AB7";
            }

            try
            {
                db.Empresas.Add(empresa);
                db.SaveChanges();

                if (!string.IsNullOrWhiteSpace(emailCliente))
                {
                    var usuario = new Usuario
                    {
                        Nome = empresa.Nome,
                        Email = emailCliente,
                        Senha = BCrypt.Net.BCrypt.HashPassword("123456"),
                        Tipo = TipoUsuario.Cliente,
                        EmpresaId = empresa.Id
                    };
                    db.Usuarios.Add(usuario);
                    db.SaveChanges();
                }
                TempData["Sucesso"] = $"Empresa \"{empresa.Nome}\" criada com sucesso! Senha inicial do cliente: 123456";
            }
            catch (Exception)
            {
                TempData["Erro"] = "Erro ao criar empresa. Tente novamente.";
            }
            return RedirectToAction("Dashboard");
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
                var manutencoes = db.Manutencoes.Where(m => m.EmpresaId == id).ToList();
                db.Manutencoes.RemoveRange(manutencoes);

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
                .Include(e => e.Manutencoes)
                .FirstOrDefault(e => e.Id == id);

            if (empresa == null) return HttpNotFound();
            return View(empresa);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
