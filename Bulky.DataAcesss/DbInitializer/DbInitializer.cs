using BookStore.DataAcess.Data;
using BookStore.Models;
using BookStore.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        
        public DbInitializer(
            UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext db )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        public void Initialize() { 
        //migration if they are not applied
        try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
        catch(Exception ex) { }
            //create a new roles if they not created 
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                // if the roles are not created, we will create admin roles as well 
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Admin",
                    Email = "admin@gmail.com",
                    Name = "Admin",
                    PhoneNumber = "1234567890",
                    StreetAddress = "1234567890",
                    State = "IL",
                    PostalCode = "700000",
                    City = "Ho Chi Minh",
                }, "Admin123@").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
        }
    }
}
