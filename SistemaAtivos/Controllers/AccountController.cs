using System.Linq;
using System.Web.Mvc;
using SistemaAtivos.Helpers;
using SistemaAtivos.Models;

namespace SistemaAtivos.Controllers
{
    // =====================================================================
    // REQUISITO 1 - AUTENTICACAO (LOGIN)
    // Controller responsavel pelo formulario de acesso e controle de Sessao
    // para proteger areas restritas do sistema.
    // =====================================================================
    public class AccountController : Controller
    {
        private AtivosContext db = new AtivosContext();

        // REQUISITO 1 - Formulario de Login (GET)
        // Exibe a tela de login. Se o usuario ja estiver autenticado,
        // redireciona para a area correspondente ao seu perfil (Admin ou Cliente).
        [HttpGet]
        public ActionResult Login()
        {
            // Verifica se ja existe uma sessao ativa
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

        // REQUISITO 1 - Processamento do Login (POST)
        // Recebe email e senha, valida as credenciais usando hash SHA-256,
        // e cria variaveis de Sessao para controlar o acesso.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string senha)
        {
            // REQUISITO 1 - SEGURANCA DE SENHAS (HASH SHA-256)
            // Regra de Ouro: Hash e de mao unica! Nao descriptografamos.
            // Aplicamos o Hash na senha digitada e comparamos com o Hash salvo no banco.
            string senhaCriptografada = CriptoHelper.HashSHA256(senha);

            // Busca o usuario pelo email E pela senha ja hasheada no banco de dados
            var usuario = db.Usuarios.FirstOrDefault(u => u.Email == email && u.Senha == senhaCriptografada);

            if (usuario == null)
            {
                ViewBag.Erro = "E-mail ou senha incorretos.";
                return View();
            }

            // REQUISITO 1 - Controle de Sessao
            // Armazena dados do usuario na Session para uso em todo o sistema
            Session["UsuarioId"]   = usuario.Id;        // Identificador unico do usuario
            Session["UsuarioNome"] = usuario.Nome;      // Nome para exibicao
            Session["UsuarioTipo"] = usuario.Tipo.ToString(); // Perfil: Admin ou Cliente
            Session["EmpresaId"]   = usuario.EmpresaId;  // Empresa vinculada (para clientes)

            // Redirecionamento baseado no perfil do usuario
            if (usuario.Tipo == TipoUsuario.Admin)
                return RedirectToAction("Dashboard", "Admin");
            else
                return RedirectToAction("Empresa", "Admin", new { id = usuario.EmpresaId });
        }

        // REQUISITO 1 - Logout / Encerramento de Sessao
        // Limpa todas as variaveis de sessao e redireciona para o login
        public ActionResult Logout()
        {
            Session.Clear();   // Remove todas as variaveis da sessao
            Session.Abandon(); // Encerra a sessao completamente
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}