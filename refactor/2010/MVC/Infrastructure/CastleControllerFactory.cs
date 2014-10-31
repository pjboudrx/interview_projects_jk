using System;
using System.Web;
using System.Web.Mvc;
using Castle.MicroKernel;

namespace MVC.Infrastructure
{
    public class CastleControllerFactory : DefaultControllerFactory
    {
        private readonly IKernel _kernel;

        public CastleControllerFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public override void ReleaseController(IController controller)
        {
            _kernel.ReleaseComponent(controller);
        }

        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, 
                    String.Format(
                        "The controller corresponding to the path '{0}' could not be identified.", 
                        requestContext.HttpContext.Request.Path));
            }
            return (IController) _kernel.Resolve(controllerType);
        }

    }
}