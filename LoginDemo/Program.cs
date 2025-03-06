using LoginDemo.Data;
using Microsoft.EntityFrameworkCore;
using LoginDemo.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("LoginDemo")));

// Add Identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Thêm Authorization để tránh lỗi
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Đảm bảo Authentication trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// Thêm phần khởi tạo vai trò và tài khoản Admin mặc định
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Tạo các vai trò Admin, Student, Teacher nếu chưa tồn tại
    string[] roleNames = { "Admin", "Student", "Teacher" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // Tạo tài khoản Admin mặc định (nếu chưa có)
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
    string adminEmail = "admin@example.com";
    string adminPassword = "Admin123456"; // Đảm bảo mật khẩu đáp ứng yêu cầu (ít nhất 8 ký tự)

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new Users
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Bỏ qua xác thực email vì bạn đã tắt RequireConfirmedEmail
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // Log lỗi nếu cần
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error creating admin user: {error.Description}");
            }
        }
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();