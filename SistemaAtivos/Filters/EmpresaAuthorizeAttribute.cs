using System.Web.Mvc;

namespace SistemaAtivos.Filters
{
    public class EmpresaAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            return httpContext.Session != null && httpContext.Session["UsuarioId"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("/Account/Login");
        }
    }
}
