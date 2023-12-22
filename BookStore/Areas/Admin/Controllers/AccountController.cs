using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Bảo mật và xác thực tại đây về vai trò của người dùng, nếu không xác thực khi truy cập vào đúng url thì
    // người không phải admin vẫn có thể chỉnh sửa được 
    [Authorize(Roles = SD.Role_Admin)]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public AccountController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            List<ApplicationUser> objAccountList = _db.ApplicationUsers.ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var  user in objAccountList) {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
            }
            return View(objAccountList);
        }
        public IActionResult Upsert(string userId)
        {
            string RoleID = _db.UserRoles.FirstOrDefault(u => u.UserId == userId).RoleId;
            RoleManagementVM RoleVM = new RoleManagementVM()
            {
                ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId),
                RoleList = _db.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                })
            };
            RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
            return View(RoleVM);
        }
        [HttpPost]
        public IActionResult Upsert(RoleManagementVM roleManagementVM)
        {
            string RoleID = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(u => u.Id == RoleID).Name;
           if(!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagementVM.ApplicationUser.Id);
                _db.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index");
        }
        public IActionResult Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser? userFromDb = _db.ApplicationUsers.Find(id);
            if (userFromDb == null)
            {
                return NotFound();
            }
            return View(userFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(string? id)
        {
            ApplicationUser? obj = _db.ApplicationUsers.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            _db.ApplicationUsers.Remove(obj);
            _db.SaveChanges();
            TempData["Success"] = "Category successfully";
            return RedirectToAction("Index");
        }
    }
}
