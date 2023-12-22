using BookStore.DataAcess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BookStore.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookStore.DataAccess.DbInitializer;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options=> options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Sử dụng để tạo vai trò quản trị viên và khách hàng 
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
//Stripe payment
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
// Thêm đoạn code sau nếu người dùng cố tình muốn đăng sử dụng đường link url product sẽ redirect tới login nếu xác thưc 
// không phải là admin thông báo từ chối 
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
//Sử dụng authentication của facebook
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "690106549932199";
    option.AppSecret = "aee5e06d5f0d9508c38662894c3848ab";
});
//
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
// Để sử dụng tính năng xác thực người dùng cần thên addrazorpage 
builder.Services.AddRazorPages();
//Add email sender after add identity role for user 
builder.Services.AddScoped<IEmailSender, EmailSender>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Tự tạo database khi sang 1 dự án mới 
SeedDatabase();
//
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}