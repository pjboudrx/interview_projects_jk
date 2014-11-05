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
        private const string ToEmailAddress = "ordering@initech.com";
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

        private void SendOrderEmailNotification(string orderEmailToAddress)
        {
            MailMessage email = new MailMessage(OrderEmailFromAddress, orderEmailToAddress);
            email.Subject = "Order submitted";
            _smtpService.SendEMail(email);
        }

        [HttpPost]
        public JsonResult SubmitOrder(List<OrderItemModel> items)
        {
            var validItems = items.Where(x => x.quantity != 0).ToList();
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var orderItems = BuildOrderItemList(unitOfWork, validItems);
                var orderId = PersistOrder(orderItems, unitOfWork);
                if (orderId > 0)
                {
                    return Json(new {OrderId = orderId, SuccessfulTransaction = true});
                }
                return Json(new { OrderId = 0, SuccessfulTransaction = false });
            }
        }

        private IList<OrderItemModel> BuildOrderItemList(IUnitOfWork unitOfWork, List<OrderItemModel> validItems)
        {
            var availableItems = unitOfWork.GetRepository<Item>().GetAll();
            correctItemPrices(validItems, availableItems);
            return validItems;
        }

        private long PersistOrder(IList<OrderItemModel> orderItems, IUnitOfWork unitOfWork)
        {
            var orderRepository = unitOfWork.GetRepository<Order>();
            var orderItemRepository = unitOfWork.GetRepository<OrderItem>();
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
            SendOrderEmailNotification(ToEmailAddress);
            return order.id;
        }

        /// <summary>
        /// MVC3 data binding does a terrible job with decimals and will generally return 0.
        /// use the available items to correct the provided prices.
        /// </summary>
        /// <param name="submittedItems"></param>
        /// <param name="availableItems"></param>
        private void correctItemPrices(List<OrderItemModel> submittedItems, IList<Item> availableItems)
        {
            var itemsDictionary = availableItems.ToDictionary(x => x.id, y => y);
            submittedItems.ForEach( item => item.price = itemsDictionary[item.item_id].price);
        }
    }
}
