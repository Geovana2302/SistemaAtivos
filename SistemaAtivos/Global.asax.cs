using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SistemaAtivos.Helpers;

namespace SistemaAtivos
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Registra binder customizado para decimal:
            // aceita tanto ponto (JS) quanto vírgula (pt-BR) como separador decimal
            ModelBinders.Binders.Add(typeof(decimal),  new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());
        }
    }
}
