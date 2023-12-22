using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<OrderHeader> orderHeaders = _db.OrderHeaders.OrderByDescending(o => o.OrderDate).ToList();
            var totalAmount = orderHeaders.Sum(o => o.OrderTotal);
            List<OrderHeader> first10rderHeaders = orderHeaders.Take(10).ToList();
            List<Product> productList = _db.Products.Take(6).ToList();
            OverviewVM overviewVM = new()
            {
                OrderHeaderList = first10rderHeaders,
                ProductList = productList,
                TotalOrderAmount = (decimal)totalAmount,
            };
            return View(overviewVM);
        }
    }
}
