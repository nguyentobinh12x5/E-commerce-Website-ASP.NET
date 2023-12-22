using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
             IEnumerable<Product> objProductList = _db.Products.Include(p => p.Category).ToList();
            return View(objProductList);
        }
        public IActionResult Details(int? productId)
        {
            Product selectedProduct = _db.Products.Find(productId);
            List<Product> relatedProducts = _db.Products
            .Where(p => p.CategoryId == selectedProduct.CategoryId && p.Id != productId)
            .ToList();
            ViewBag.RelatedProducts = relatedProducts;
            ShoppingCart cart = new()
            {
                Product = selectedProduct,
                Count = 1,
            };
            return View(cart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            ShoppingCart cartFromDb = _db.ShoppingCarts.FirstOrDefault(u=>u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _db.ShoppingCarts.Update(cartFromDb);
            } else
            {
                _db.ShoppingCarts.Add(shoppingCart);
            }
            
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult About()
        {
            return View();  
        }
        public IActionResult Shop(int id)
        {
            IEnumerable<Product> objProductList;

            if (id > 0)
            {
                // Lọc sản phẩm theo danh mục có id tương ứng
                objProductList = _db.Products
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == id)
                    .ToList();
            }
            else
            {
                // Không có id được chuyển vào, trả về tất cả sản phẩm
                objProductList = _db.Products.Include(p => p.Category).ToList();
            }

            return View(objProductList);
        }
        public IActionResult Blog()
        {
            IEnumerable<Blog> objBlogList = _db.Blogs.ToList();
            return View(objBlogList);
        }
        public IActionResult BlogDetail(int id)
        {
            Blog blog = _db.Blogs.Include(c => c.ApplicationUser).FirstOrDefault( b => b.Id == id);
            return View(blog);
        }
    }
}
