using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
		public CartController(ApplicationDbContext db)
        {
            _db = db;
		}
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new() { ShoppingCartList = _db.ShoppingCarts.Include(sc => sc.Product).Where(u => u.ApplicationUserId == userId), OrderHeader= new() };
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                double price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            // Phương thức where sử dụng trả về tập hop các phần tử trong mảng 
            ShoppingCartVM = new() { ShoppingCartList = _db.ShoppingCarts.Include(sc => sc.Product).Where(u => u.ApplicationUserId == userId) , OrderHeader= new()};
            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUsers.First(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.Number;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                double price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (price * cart.Count);
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST(string paymentMethod)
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartVM.ShoppingCartList = _db.ShoppingCarts.Include(sc => sc.Product).Where(u => u.ApplicationUserId == userId);
			ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            //ApplicationUser applicationUser = _db.ApplicationUsers.First(u => u.Id == userId);
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				double price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (price * cart.Count);
			}
			ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            _db.OrderHeaders.Add(ShoppingCartVM.OrderHeader);
            _db.SaveChanges();
            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Product.Price,
                    Count = cart.Count,
                };
                orderDetails.Add(orderDetail);
			}
            //Add range nghĩa là add nhiều object vào trong database 
            _db.OrderDetails.AddRange(orderDetails);
            _db.SaveChanges();
            // Stripe Payment 
            if(paymentMethod == "stripe")
            {
                var domain = "https://localhost:7163/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Product.PriceDiscount),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                var orderSession = _db.OrderHeaders.Find(ShoppingCartVM.OrderHeader.Id);
                orderSession.PaymentIntendId = session.PaymentIntentId;
                orderSession.SessionId = session.Id; 
                _db.SaveChanges();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            // Stripe payment
            return RedirectToAction(nameof(OrderConfirmation), new {id = ShoppingCartVM.OrderHeader.Id, paymentMethod});
		}
        public IActionResult OrderConfirmation(int id, string paymentMethod) {
            OrderHeader orderHeader = _db.OrderHeaders.Include(u => u.ApplicationUser).FirstOrDefault(u => u.Id == id);
            if(paymentMethod == "stripe")
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    orderHeader.PaymentIntendId = session.PaymentIntentId;
                    orderHeader.SessionId = session.Id;
                    orderHeader.OrderStatus = SD.StatusApproved;
                    orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    _db.SaveChanges();
                }
            }
            List<ShoppingCart> shoppingCarts = _db.ShoppingCarts.Where(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _db.ShoppingCarts.RemoveRange(shoppingCarts);
            _db.SaveChanges();
            return View(id);
        }
		public IActionResult Plus(int cartId)
        {
            // Sử dụng var cartFromDb or ShoppingCart đều được 
            // Nên sử dụng cái nào hơn khi muốn tìm 
            ShoppingCart? cartFromDb = _db.ShoppingCarts.First(c => c.Id == cartId);
            cartFromDb.Count += 1;
            _db.ShoppingCarts.Update(cartFromDb);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _db.ShoppingCarts.FirstOrDefault(c => c.Id == cartId);
            if(cartFromDb.Count <= 1)
            {
                _db.ShoppingCarts.Remove(cartFromDb);
            } else
            {
                cartFromDb.Count -= 1;
            }
            _db.ShoppingCarts.Update(cartFromDb);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _db.ShoppingCarts.FirstOrDefault(c => c.Id == cartId);
            _db.ShoppingCarts.Remove(cartFromDb);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            return shoppingCart.Product.PriceDiscount;
        }
    }
}
