using LoginDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LoginDemo.Controllers
{
    // [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class AdminController : Controller
    {
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // Hiển thị danh sách người dùng
        public IActionResult Index()
        {
            var users = userManager.Users.ToList();
            return View(users);
        }

        // Hiển thị form tạo người dùng mới
        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        // Xóa tài khoản người dùng
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                await userManager.DeleteAsync(user);
                TempData["Success"] = "User deleted successfully!";
            }
            else
            {
                TempData["Error"] = "User not found!";
            }
            return RedirectToAction("Index");
        }

        // Thay đổi Role của người dùng
        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var currentRoles = await userManager.GetRolesAsync(user);
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, newRole);

                user.Role = newRole;
                await userManager.UpdateAsync(user);

                TempData["Success"] = "User role updated successfully!";
            }
            else
            {
                TempData["Error"] = "User not found!";
            }
            return RedirectToAction("Index");
        }

        // Tạo người dùng mới với Role Student/Teacher
        [HttpPost]
        public async Task<IActionResult> CreateUser(string FullName, string Email, string Password, string Role)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Role))
            {
                TempData["Error"] = "All fields are required!";
                return RedirectToAction("Index");
            }

            // Kiểm tra xem Email đã tồn tại chưa
            var existingUser = await userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                TempData["Error"] = $"Email {Email} already exists!";
                return RedirectToAction("Index");
            }

            // Kiểm tra Role hợp lệ trước khi tạo
            if (!await roleManager.RoleExistsAsync(Role))
            {
                var newRole = new IdentityRole(Role);
                await roleManager.CreateAsync(newRole);
            }

            var user = new Users
            {
                UserName = Email,
                Email = Email,
                FullName = FullName,
                Role = Role // Gán Role vào User
            };

            var result = await userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, Role);
                await userManager.UpdateAsync(user);

                TempData["Success"] = "User created successfully!";
            }
            else
            {
                TempData["Error"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }
    }
}
