using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;
using MVC.Models.Order;

namespace MVC.Controllers
{
    public class OrderController : Controller
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;

        public OrderController(IUnitOfWorkFactory unitOfWorkFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
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

                //Send email
                MailMessage email = new MailMessage("peter@initech.com", "ordering@initech.com");
                email.Subject = "Order submitted";

                SmtpClient client = new SmtpClient("localhost");

                try
                {
                    client.Send(email);
                }
// ReSharper disable once EmptyGeneralCatchClause
                catch (Exception)
                {
                    //It is ok that it doesn't actually send the email for this project    
                }

                ViewData["order"] = order;
                return View("ThankYou");
            }
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
