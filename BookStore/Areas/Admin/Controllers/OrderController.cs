using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Bảo mật và xác thực tại đây về vai trò của người dùng, nếu không xác thực khi truy cập vào đúng url thì
    // người không phải admin vẫn có thể chỉnh sửa được 
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        public OrderController(ApplicationDbContext db)
        {
            _db = db;
        }
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public static SelectListItem[] StatusList = new SelectListItem[]
            {
                new SelectListItem { Text = SD.StatusPending, Value = SD.StatusPending },
                new SelectListItem { Text = SD.StatusInProcess, Value = SD.StatusInProcess },
                new SelectListItem { Text = SD.StatusApproved, Value = SD.StatusApproved },
                new SelectListItem { Text = SD.StatusShipped, Value = SD.StatusShipped },
                new SelectListItem { Text = SD.StatusCancelled, Value = SD.StatusCancelled },
                new SelectListItem { Text = SD.StatusRefunded, Value = SD.StatusRefunded },
            };
        public static SelectListItem[] PaymentList = new SelectListItem[]
           {
                new SelectListItem { Text = SD.PaymentStatusPending, Value = SD.PaymentStatusPending },
                new SelectListItem { Text = SD.PaymentStatusApproved, Value = SD.PaymentStatusApproved },
                new SelectListItem { Text = SD.PaymentStatusRejected, Value = SD.PaymentStatusRejected },
                new SelectListItem { Text = SD.PaymentStatusDelayedPayment, Value = SD.PaymentStatusDelayedPayment },
           };
        public IActionResult Index(string status)
        {
            IEnumerable<OrderHeader> objOrderList = _db.OrderHeaders.ToList();
            switch (status) {
                case "pending":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusPending);
                    break;
                case "inprocess":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "approved":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "cancelled":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusCancelled);
                    break;
                case "refunded":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusRefunded);
                    break;
                case "shipped":
                    objOrderList = objOrderList.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                default: 
                    break;
            }
            return View(objOrderList);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> UserList = _db.ApplicationUsers.ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            ViewBag.UserList = UserList;
            ViewBag.StatusList = StatusList;
            ViewBag.PaymentList = PaymentList;
            if (id == null || id == 0)
            {
                return View();
            }
            else
            {
                //update 
                OrderHeader? orderHeaderFromDb = _db.OrderHeaders.Find(id);
                return View(orderHeaderFromDb);
            }
        }
        [HttpPost]
        public IActionResult Upsert(OrderHeader orderHeader)
        {
            if (orderHeader.Id == null)
            {
                _db.OrderHeaders.Add(orderHeader);
            }
            else
            {
                _db.OrderHeaders.Update(orderHeader);
            }
            _db.SaveChanges();
            TempData["Success"] = "Tạo thành công đơn hàng";
            return RedirectToAction("Index");
        }
        public IActionResult Detail(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId),
                OrderDetail = _db.OrderDetails.Include(u => u.Product).Where(u => u.OrderHeaderId == orderId)
            };
            return View(orderVM);
        }
        public IActionResult Status(int id)
        {
            OrderHeader? orderHeaderFromDb = _db.OrderHeaders.Find(id);
            ViewBag.StatusList = StatusList;
            ViewBag.PaymentList = PaymentList;
            return View(orderHeaderFromDb);
        }
        [HttpPost]
        public IActionResult UpdateStatus(int id, string OrderStatus, string PaymentStatus)
        {
            var StatusUpdate = _db.OrderHeaders.Find(id);
            if(PaymentStatus == null)
            {
                StatusUpdate.OrderStatus = OrderStatus;
            } else if (OrderStatus == null)
            {
                StatusUpdate.PaymentStatus = PaymentStatus;
            } else
            {
                StatusUpdate.OrderStatus = OrderStatus;
                StatusUpdate.PaymentStatus = PaymentStatus;
            }
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId),
                OrderDetail = _db.OrderDetails.Include(u => u.Product).Where(u => u.OrderHeaderId == orderId)
            };
            return View(orderVM);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int orderId)
        {
            var orderHeader = _db.OrderHeaders.FirstOrDefault(u => u.Id == orderId);
            if (orderHeader == null)
            {
                return NotFound();
            }
            var orderDetails = _db.OrderDetails.Where(u => u.OrderHeaderId == orderId);

            _db.OrderDetails.RemoveRange(orderDetails);

            _db.OrderHeaders.Remove(orderHeader);
            _db.SaveChanges();

            return RedirectToAction("Index"); 
        }
    }
}