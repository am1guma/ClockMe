using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using ClockMe.App_Start;

namespace ClockMe
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Global.Init();
        }
    }
}
