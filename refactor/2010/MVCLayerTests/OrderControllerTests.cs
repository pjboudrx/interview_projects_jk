using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MVC.Controllers;
using MVC.Models.Order;
using MVC.Services.Interfaces;
using MvcContrib.TestHelper;
using MVCLayerTests.Annotations;
using Rhino.Mocks;

namespace MVCLayerTests
{
    [TestClass]
    public class OrderControllerTests
    {
        private static readonly List<OrderItem> FixedOrderItems = new List<OrderItem>
        {
            new OrderItem {item_id = 0, price = 1, quantity = 1},
            new OrderItem {item_id = 1, price = 4, quantity = 2},
            new OrderItem {item_id = 2, price = 6.28m, quantity = 2}
        };

        private static readonly List<Order> Orders = new List<Order>
        {
            new Order{id = 0, items = FixedOrderItems.Where( oi => oi.item_id <= 1).ToList()},
            new Order{id = 1, items = FixedOrderItems.Where( oi => oi.item_id > 1).ToList()}
        };

        private static readonly List<Item> FixedItems = new List<Item>
        {
            new Item {description = "Test Item 1", id = 0, price = 1},
            new Item {description = "Test Item 2", id = 1, price = 2},
            new Item {description = "Test Item 3", id = 2, price = 3.14m}
        };

        [TestMethod]
        public void DefaultIndexActionReturnsViewResultTest()
        {
            //Setup
            TestControllerBuilder builder;
            Mocks localMocks = new Mocks();
            var controller = BuildSimpleMockedOrderController(ref localMocks, out builder);
            //Act
            ActionResult result = controller.Index();
            //Assert
            result.AssertViewRendered();
            localMocks.ItemRepository.AssertWasCalled( x => x.GetAll() );
            if (controller.HttpContext.Session != null)
                Assert.IsInstanceOfType(controller.HttpContext.Session["order_items"], typeof(List<OrderItemModel>));
        }

        [TestMethod]
        public void ViewPastOrdersActionReturnsViewResultTest()
        {
            //Setup
            TestControllerBuilder builder;
            Mocks localMocks = new Mocks();
            var controller = BuildSimpleMockedOrderController(ref localMocks, out builder);
            //Act
            ActionResult result = controller.ViewPastOrders();
            //Assert
            result.AssertViewRendered().ForView("PastOrders");
            localMocks.OrderRepository.AssertWasCalled(x => x.GetAll());
            if (controller.HttpContext.Session != null)
                Assert.AreEqual(controller.ViewData["orders"], Orders);
        }

        [TestMethod]
        public void SubmitOrderActionReturnsSuccessfulJsonResponse()
        {
            //Setup
            TestControllerBuilder builder;
            Mocks localMocks = new Mocks();
            var controller = BuildSimpleMockedOrderController(ref localMocks, out builder);
            //overwrite the existing save mock so that it'll act like the persist was successful
            localMocks.OrderRepository.BackToRecord(BackToRecordOptions.All);
            localMocks.OrderRepository.Replay();
            localMocks.OrderRepository.Stub(x => x.Save(Arg<Order>.Is.Anything)).Return(true).WhenCalled(_ =>
            {
                var order = (Order)_.Arguments[0];
                order.id = 1;
            });
            if (controller.HttpContext.Session != null)
            {
                var successfulTransaction = BuildAndSubmitOrderFromFixedOrderItems(controller, localMocks);
                Assert.IsTrue(successfulTransaction);
            }
        }

        [TestMethod]
        public void SubmitOrderActionReturnsUnsuccessfulJsonResponse()
        {
            //Setup
            TestControllerBuilder builder;
            Mocks localMocks = new Mocks();
            var controller = BuildSimpleMockedOrderController(ref localMocks, out builder);
            //overwrite the existing save mock so that it'll act like the persist was successful
            localMocks.OrderRepository.BackToRecord(BackToRecordOptions.All);
            localMocks.OrderRepository.Replay();
            localMocks.OrderRepository.Stub(x => x.Save(Arg<Order>.Is.Anything)).Return(false).WhenCalled(_ =>
            {
                var order = (Order)_.Arguments[0];
                order.id = 0;
            });
            if (controller.HttpContext.Session != null)
            {
                var successfulTransaction = BuildAndSubmitOrderFromFixedOrderItems(controller, localMocks);
                Assert.IsFalse(successfulTransaction);
            }
        }

        private static bool BuildAndSubmitOrderFromFixedOrderItems(OrderController controller, Mocks localMocks)
        {
            var orderItems =
                FixedOrderItems.Select(x =>
                    new OrderItemModel
                    {
                        description = FixedItems.First(fi => fi.id == x.item_id).description,
                        item_id = x.item_id,
                        quantity = x.quantity,
                        price = x.price
                    }).ToList();
            //Act
            JsonResult result = controller.SubmitOrder(orderItems);
            //Assert

            //Assert all OrderItems are saved
            localMocks.OrderItemRepository.AssertWasCalled(x => x.Save(Arg<OrderItem>.Is.Anything),
                options => options.Repeat.Times(FixedOrderItems.Count));
            //Assert that the order was saved
            localMocks.OrderRepository.AssertWasCalled(x => x.Save(Arg<Order>.Is.Anything), options => options.Repeat.Once());
            //Assert email was sent
            localMocks.SMTPService.AssertWasCalled(x => x.SendEMail(Arg<MailMessage>.Is.Anything));

            //Verify proper successful json is returned
            Assert.IsInstanceOfType(result, typeof (JsonResult));
            var data = result.Data;
            //I do wish reflection wasn't necessary but if we're not going to use web api for our client side api methods c'est la vie
            var successfulTransaction =
                (bool) data.GetType().GetProperty("SuccessfulTransaction").GetValue(data, null);
            return successfulTransaction;
        }

        private OrderController BuildSimpleMockedOrderController(ref Mocks localMocks, out TestControllerBuilder builder)
        {
            builder = new TestControllerBuilder();
            localMocks.UnitOfWorkFactory = GenerateMockUnitOfWorkFactory( ref localMocks );
            localMocks.SMTPService = GenerateMockSMTPService();

            var controller = new OrderController(localMocks.UnitOfWorkFactory, localMocks.SMTPService);
            builder.InitializeController(controller);
            return controller;
        }


        private ISMTPService GenerateMockSMTPService()
        {
            var smtpServiceMock = MockRepository.GenerateStub<ISMTPService>();
            smtpServiceMock.Stub(x => x.SendEMail(Arg<MailMessage>.Is.Anything)).Return(true);

            return smtpServiceMock;
        }

        private IUnitOfWorkFactory GenerateMockUnitOfWorkFactory(ref Mocks localMocks)
        {
            localMocks.UnitOfWork = GenerateBasicUnitOfWork( ref localMocks );

            var uowFactory = MockRepository.GenerateStub<IUnitOfWorkFactory>();
            uowFactory.Stub(x => x.Create()).Return(localMocks.UnitOfWork);
            return uowFactory;
        }

        private IUnitOfWork GenerateBasicUnitOfWork(ref Mocks localMocks)
        {
            localMocks.OrderRepository = GenerateBasicOrderRepository();
            localMocks.OrderItemRepository = GenerateBasicOrderItemRepository();
            localMocks.ItemRepository = GenerateBasicItemRepository();

            var unitOfWorkMock = MockRepository.GenerateStub<IUnitOfWork>();
            unitOfWorkMock.Stub(x => x.GetRepository<Order>()).Return(localMocks.OrderRepository);
            unitOfWorkMock.Stub(x => x.GetRepository<OrderItem>()).Return(localMocks.OrderItemRepository);
            unitOfWorkMock.Stub(x => x.GetRepository<Item>()).Return(localMocks.ItemRepository);
            unitOfWorkMock.Stub(x => x.Commit()).Return(true);
            return unitOfWorkMock;
        }

        private IRepository<Item> GenerateBasicItemRepository()
        {
            var itemRepo = MockRepository.GenerateStub<IRepository<Item>>();
            itemRepo.Stub(x => x.Dispose());
            itemRepo.Stub(x => x.GetAll()).Return(
                FixedItems);
            itemRepo.Stub(x => x.Save(Arg<Item>.Is.Anything)).Return(true);
            return itemRepo;
        }

        private IRepository<OrderItem> GenerateBasicOrderItemRepository()
        {
            var orderItemRepo = MockRepository.GenerateStub<IRepository<OrderItem>>();
            orderItemRepo.Stub(x => x.GetAll()).Return(
                FixedOrderItems);
            orderItemRepo.Stub(x => x.Save(Arg<OrderItem>.Is.Anything)).Return(true);
            return orderItemRepo;
        }

        private IRepository<Order> GenerateBasicOrderRepository()
        {
            var orderRepo = MockRepository.GenerateStub<IRepository<Order>>();
            orderRepo.Stub(x => x.GetAll()).Return(
                Orders);
            orderRepo.Stub(x => x.Save(Arg<Order>.Is.Anything)).Return(true);
            return orderRepo;
        }


        private class Mocks
        {
            [UsedImplicitly] public IRepository<Order> OrderRepository;
            [UsedImplicitly] public IRepository<OrderItem> OrderItemRepository;
            [UsedImplicitly] public IRepository<Item> ItemRepository;
            [UsedImplicitly] public IUnitOfWorkFactory UnitOfWorkFactory;
            [UsedImplicitly] public IUnitOfWork UnitOfWork;
            [UsedImplicitly] public ISMTPService SMTPService;
        }
    }
}
