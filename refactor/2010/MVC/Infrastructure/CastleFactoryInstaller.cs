using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Domain.DomainClasses;
using Domain.Repository.Implementation;
using Domain.Repository.Interfaces;

namespace MVC.Infrastructure
{
    public class CastleFactoryInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Kernel.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For<IUnitOfWorkFactory>().AsFactory());
            
        }
    }
}