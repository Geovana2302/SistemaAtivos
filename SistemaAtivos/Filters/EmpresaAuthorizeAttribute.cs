using System.Web.Mvc;

namespace SistemaAtivos.Filters
{
    // =====================================================================
    // REQUISITO 1 - AUTENTICACAO (PROTECAO DE AREAS RESTRITAS)
    // Filtro de autorizacao customizado que protege os Controllers.
    // Verifica se existe uma sessao ativa antes de permitir o acesso.
    // Uso: [EmpresaAuthorize] decorando os Controllers protegidos.
    // =====================================================================
    public class EmpresaAuthorizeAttribute : AuthorizeAttribute
    {
        // Verifica se o usuario esta autenticado checando a sessao
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            return httpContext.Session != null && httpContext.Session["UsuarioId"] != null;
        }

        // Se nao autenticado, redireciona para a tela de login
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Account/Login");
        }
    }
}
