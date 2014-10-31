using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Domain;
using MVC.Models.Order;

namespace MVC.Controllers
{
    public class OrderController : Controller
    {
        public ActionResult Index()
        {
            var items = new List<Item>()
                            {
                                new Item(){description = "Red Stapler",price = 50, id = 1},
                                new Item(){description = "TPS Report", price = 3, id = 2},
                                new Item(){description = "Printer", price = 400, id = 3},
                                new Item(){description = "Baseball bat", price = 80, id = 4},
                                new Item(){description = "Michael Bolton CD", price = 12, id = 5}
                            };

            ViewData["items"] = items;
            Session["order_items"] = new List<OrderItemModel>();

            return View();
        }

        public ActionResult ViewPastOrders()
        {
            var order_repository = (OrderRepository)System.Web.HttpContext.Current.Application["order_repository"];
            var orders = order_repository.get_all();

            ViewData["orders"] = orders;

            return View("PastOrders");
        }

        [HttpPost]
        public ActionResult Save(FormCollection form_collection)
        {
            //Save Order
            var order_repository = (OrderRepository)System.Web.HttpContext.Current.Application["order_repository"];
            var order_items = (IList<OrderItemModel>)Session["order_items"];
            var order = new Order();

            foreach (var order_item in order_items)
            {
                var item = new OrderItem {item_id = order_item.item_id, quantity = order_item.quantity, price = order_item.price};
                order.items.Add(item);
            }

            order_repository.save(order);

            //Send email
            MailMessage email = new MailMessage("peter@initech.com","ordering@initech.com");
            email.Subject = "Order submitted";

            SmtpClient client = new SmtpClient("localhost");

            try
            {
                client.Send(email);
            }
            catch(Exception exception)
            {
                //It is ok that it doesn't actually send the email for this project    
            }

            ViewData["order"] = order;
            return View("ThankYou");
        }

        [HttpPost]
        public ActionResult AddToOrder(FormCollection form_collection)
        {
            var items = new List<Item>()
                            {
                                new Item(){description = "Red Stapler",price = 50, id = 1},
                                new Item(){description = "TPS Report", price = 3, id = 2},
                                new Item(){description = "Printer", price = 400, id = 3},
                                new Item(){description = "Baseball bat", price = 80, id = 4},
                                new Item(){description = "Michael Bolton CD", price = 12, id = 5}
                            };

            ViewData["items"] = items;

            IList<OrderItemModel> order_items = (IList<OrderItemModel>)Session["order_items"];
            if (order_items == null)
            {
                order_items = new List<OrderItemModel>();
            }

            int item_id = 0;
            int item_quantity = 0;

            foreach (var key in form_collection.Keys)
            {
                if (key.ToString().StartsWith("item_id"))
                {
                    item_id = int.Parse(form_collection[key.ToString()]);
                }

                if (key.ToString().StartsWith("item_quantity"))
                {
                    item_quantity = int.Parse(form_collection[key.ToString()]);
                }
            }

            var item = items.First(x => x.id == item_id);
            var order_item = new OrderItemModel
                                    {
                                        item_id=item.id,
                                        price=item.price,
                                        description = item.description,
                                        quantity = item_quantity
                                    };

            order_items.Add(order_item);

            Session["order_items"] = order_items;

            return View("Index");
        }
    }
}
