using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Domain.DomainClasses;
using Domain.Repository.Implementation;
using Domain.Repository.Interfaces;

namespace MVC.Infrastructure
{
    public class CastleDALInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IRepository<Order>>().ImplementedBy<OrderRepository>().LifestyleSingleton());
            container.Register(Component.For<IRepository<Item>>().ImplementedBy<ItemRepository>().LifestyleSingleton());
            container.Register(Component.For<IRepository<OrderItem>>().ImplementedBy<OrderItemRepository>().LifestyleSingleton());
            container.Register(Component.For<IUnitOfWork>().ImplementedBy<UnitOfWork>().LifestyleTransient());
        }
    }
}