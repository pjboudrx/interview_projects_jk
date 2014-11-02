using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MVC.Controllers;
using MvcContrib.TestHelper;

namespace MVCLayerTests
{
    [TestClass]
    public class HomeControllerTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            HomeController controller = new HomeController();
            var result = controller.Index();
            result.AssertViewRendered();
        }
    }
}
