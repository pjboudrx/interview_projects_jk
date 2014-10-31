using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using MVC.Services.Implementation;
using MVC.Services.Interfaces;

namespace MVC.Infrastructure
{
    public class CastleServiceInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<ISMTPService>().ImplementedBy<SMTPService>().LifestylePerWebRequest());
            
        }
    }
}