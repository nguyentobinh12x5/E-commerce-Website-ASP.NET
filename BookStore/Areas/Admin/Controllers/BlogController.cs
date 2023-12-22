using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BlogController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Blog> objBlogList = _db.Blogs.Include(u => u.ApplicationUser).ToList();
            return View(objBlogList);
        }
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View();
            }
            else
            {
                //update 
                Blog? blogFromDb = _db.Blogs.Find(id);
                return View(blogFromDb);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Blog obj, IFormFile? file)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            obj.CreateDate = System.DateTime.Now;
            obj.ApplicationUserId = userId;
            // Tạm thời đã xóa xác thực form do lỗi cần kiểm tra lại bước này 
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\blog");
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
                obj.ImageUrl1 = @"\images\blog\" + fileName;
            }

            if (obj.Id == null)
            {
                _db.Blogs.Add(obj);
            }
            else
            {
                _db.Blogs.Update(obj);
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
            Blog? blogFromDb = _db.Blogs.Find(id);
            if (blogFromDb == null)
            {
                return NotFound();
            }
            return View(blogFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Blog obj = _db.Blogs.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.Blogs.Remove(obj);
            _db.SaveChanges();
            TempData["Success"] = "Category successfully";
            return RedirectToAction("Index");
        }
    }
}
