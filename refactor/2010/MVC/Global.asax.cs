using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;
using MVC.Infrastructure;

namespace MVC
{
using Domain.Repository.Implementation;
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer _container;

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        private static void BootstrapCastleContainer()
        {
            _container = new WindsorContainer().Install(FromAssembly.This());
            var controllerFactory = new CastleControllerFactory(_container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            BootstrapCastleContainer();
            BootstrapRepositories();
        }

        private void BootstrapRepositories()
        {
            using (var unitOfWork = _container.Resolve<IUnitOfWork>())
            {
                var itemRepository = unitOfWork.GetRepository<Item>();
                itemRepository.Save(new Item {description = "Red Stapler", price = 50, id = 1});
                itemRepository.Save(new Item {description = "TPS Report", price = 3, id = 2});
                itemRepository.Save(new Item {description = "Printer", price = 400, id = 3});
                itemRepository.Save(new Item {description = "Baseball bat", price = 80, id = 4});
                itemRepository.Save(new Item { description = "Michael Bolton CD", price = 12, id = 5 });
            }
        }
    }
}