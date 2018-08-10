using BestLot.BusinessLogicLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BestLot.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private ILotSalesHandler lotSalesHandler = BusinessLogicLayer.LogicDependencyResolver.ResolveLotSalesHandler(60000, 30000);
        protected void Application_Start()
        {
            lotSalesHandler.RunSalesHandler();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
