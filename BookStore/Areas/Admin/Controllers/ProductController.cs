using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _db.Products.Include(p => p.Category).ToList();
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _db.Categories.ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString(),
            });
            ViewBag.CategoryList = CategoryList;
            if (id == null || id == 0)
            {
                return View();
            }
            else
            {
                //update 
                Product? productFromDb = _db.Products.Find(id);
                return View(productFromDb);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Product obj, IFormFile? file, IFormFile? file2, IFormFile file3, IFormFile file4)
        {
            // Tạm thời đã xóa xác thực form do lỗi cần kiểm tra lại bước này 
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");
                if (!string.IsNullOrEmpty(obj.ImageUrl1))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, obj.ImageUrl1.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
                obj.ImageUrl1 = @"\images\product\" + fileName;
            }
            if (file2 != null)
            {
                string fileName2 = Guid.NewGuid().ToString() + Path.GetExtension(file2.FileName);
                string productPath2 = Path.Combine(wwwRootPath, @"images\product");

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(obj.ImageUrl2))
                {
                    var oldImagePath2 = Path.Combine(wwwRootPath, obj.ImageUrl2.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath2))
                    {
                        System.IO.File.Delete(oldImagePath2);
                    }
                }

                // Lưu ảnh mới
                using (var fileStream2 = new FileStream(Path.Combine(productPath2, fileName2), FileMode.Create))
                {
                    file2.CopyTo(fileStream2);
                }
                obj.ImageUrl2 = @"\images\product\" + fileName2;
            }
            if (file3 != null)
            {
                string fileName3 = Guid.NewGuid().ToString() + Path.GetExtension(file3.FileName);
                string productPath3 = Path.Combine(wwwRootPath, @"images\product");

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(obj.ImageUrl3))
                {
                    var oldImagePath3 = Path.Combine(wwwRootPath, obj.ImageUrl3.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath3))
                    {
                        System.IO.File.Delete(oldImagePath3);
                    }
                }

                // Lưu ảnh mới
                using (var fileStream3 = new FileStream(Path.Combine(productPath3, fileName3), FileMode.Create))
                {
                    file3.CopyTo(fileStream3);
                }
                obj.ImageUrl3 = @"\images\product\" + fileName3;
            }
            if (file4 != null)
            {
                string fileName4 = Guid.NewGuid().ToString() + Path.GetExtension(file4.FileName);
                string productPath4 = Path.Combine(wwwRootPath, @"images\product");

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(obj.ImageUrl4))
                {
                    var oldImagePath4 = Path.Combine(wwwRootPath, obj.ImageUrl4.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath4))
                    {
                        System.IO.File.Delete(oldImagePath4);
                    }
                }

                // Lưu ảnh mới
                using (var fileStream4 = new FileStream(Path.Combine(productPath4, fileName4), FileMode.Create))
                {
                    file4.CopyTo(fileStream4);
                }
                obj.ImageUrl4 = @"\images\product\" + fileName4;
            }

            if (obj.Id == null)
            {
                _db.Products.Add(obj);
            }
            else
            {
                _db.Products.Update(obj);
            }

            _db.SaveChanges();
            TempData["Success"] = "Create a Product successfully";
            return RedirectToAction("Index");
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? productFromDb = _db.Products.Find(id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product? obj = _db.Products.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Products.Remove(obj);
            _db.SaveChanges();
            TempData["Success"] = "Category successfully";
            return RedirectToAction("Index");
        }
    }
}