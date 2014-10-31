using System.Collections.Generic;
using System.Linq;
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
            var items = new List<Item>
            {
                                new Item {description = "Red Stapler",price = 50, id = 1},
                                new Item {description = "TPS Report", price = 3, id = 2},
                                new Item {description = "Printer", price = 400, id = 3},
                                new Item {description = "Baseball bat", price = 80, id = 4},
                                new Item {description = "Michael Bolton CD", price = 12, id = 5}
                            };

            ViewData["items"] = items;
            Session["order_items"] = new List<OrderItemModel>();

            return View();
        }

        public ActionResult ViewPastOrders()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {

                var orders = unitOfWork.GetRepository<Order>().GetAll();

                ViewData["orders"] = orders;

                return View("PastOrders");
            }
        }

        [HttpPost]
        public ActionResult Save(FormCollection formCollection)
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
        public ActionResult AddToOrder(FormCollection formCollection)
        {
            var items = new List<Item>
            {
                                new Item {description = "Red Stapler",price = 50, id = 1},
                                new Item {description = "TPS Report", price = 3, id = 2},
                                new Item {description = "Printer", price = 400, id = 3},
                                new Item {description = "Baseball bat", price = 80, id = 4},
                                new Item {description = "Michael Bolton CD", price = 12, id = 5}
                            };

            ViewData["items"] = items;

            IList<OrderItemModel> orderItems = (IList<OrderItemModel>)Session["order_items"];
            if (orderItems == null)
            {
                orderItems = new List<OrderItemModel>();
            }

            int itemId = 0;
            int itemQuantity = 0;

            foreach (var key in formCollection.Keys)
            {
                if (key.ToString().StartsWith("item_id"))
                {
                    itemId = int.Parse(formCollection[key.ToString()]);
                }

                if (key.ToString().StartsWith("item_quantity"))
                {
                    itemQuantity = int.Parse(formCollection[key.ToString()]);
                }
            }

            var item = items.First(x => x.id == itemId);
            var orderItem = new OrderItemModel
                                    {
                                        item_id=item.id,
                                        price=item.price,
                                        description = item.description,
                                        quantity = itemQuantity
                                    };

            orderItems.Add(orderItem);

            Session["order_items"] = orderItems;

            return View("Index");
        }
    }
}
