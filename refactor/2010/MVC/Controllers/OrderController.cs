using System.Collections.Generic;
using System.Net.Mail;
using System.Web.Mvc;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;
using MVC.Models.Order;
using MVC.Services.Interfaces;

namespace MVC.Controllers
{
    public class OrderController : Controller
    {
        private const string OrderEmailFromAddress = "peter@initech.com";
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private ISMTPService _smtpService;

        public OrderController(IUnitOfWorkFactory unitOfWorkFactory, ISMTPService smtpService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _smtpService = smtpService;
        }

        public ActionResult Index()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                ViewData["items"] = unitOfWork.GetRepository<Item>().GetAll();
                Session["order_items"] = new List<OrderItemModel>();
                return View();
            }
        }

        public ActionResult ViewPastOrders()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                ViewData["orders"] = unitOfWork.GetRepository<Order>().GetAll();
                return View("PastOrders");
            }
        }

        [HttpPost]
        public ActionResult Save()
        {
            //Save Order
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var orderRepository = unitOfWork.GetRepository<Order>();
                var orderItemRepository = unitOfWork.GetRepository<OrderItem>();
                var orderItems = (IList<OrderItemModel>)Session["order_items"];
                var order = new Order();

                foreach (var orderItem in orderItems)
                {
                    var item = new OrderItem
                    {
                        item_id = orderItem.item_id,
                        quantity = orderItem.quantity,
                        price = orderItem.price
                    };
                    orderItemRepository.Save(item);
                    order.items.Add(item);
                }

                orderRepository.Save(order);

                //TODO:  Gather this from user input
                SendOrderEmailNotification("ordering@initech.com");

                ViewData["order"] = order;
                return View("ThankYou");
            }
        }

        private void SendOrderEmailNotification(string orderEmailToAddress)
        {
            MailMessage email = new MailMessage(OrderEmailFromAddress, orderEmailToAddress);
            email.Subject = "Order submitted";
            _smtpService.SendEMail(email);
        }

        [HttpPost]
        public ActionResult AddToOrder(OrderItemModel item)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                IList<Item> items = unitOfWork.GetRepository<Item>().GetAll();
                ViewData["items"] = items;
                IList<OrderItemModel> orderItems = (IList<OrderItemModel>) Session["order_items"];
                if (orderItems == null)
                {
                    orderItems = new List<OrderItemModel>();
                }
                orderItems.Add(item);
                Session["order_items"] = orderItems;
                return View("Index");
            }
        }
    }
}
