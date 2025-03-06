using LoginDemo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginDemo.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới truy cập được
    public class AdminController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // ViewModel để truyền dữ liệu cho view Index
        public class UserViewModel
        {
            public string Id { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public IList<string> Roles { get; set; }
        }

        // Action để hiển thị trang chính của Admin
        [HttpGet]
        public async Task<IActionResult> Index(string searchQuery = null)
        {
            try
            {
                _logger.LogInformation("Admin accessed the user management page.");

                // Lấy danh sách người dùng
                var usersQuery = _userManager.Users.AsQueryable();
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    searchQuery = searchQuery.ToLower();
                    usersQuery = usersQuery.Where(u => u.Email.ToLower().Contains(searchQuery) ||
                                                      (u.FullName != null && u.FullName.ToLower().Contains(searchQuery)));
                }

                var users = await usersQuery.ToListAsync();

                // Lấy vai trò của tất cả người dùng trong một lần truy vấn
                var userViewModels = new List<UserViewModel>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Roles = roles
                    });
                }

                return View(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the user list.");
                return StatusCode(500, "An error occurred while retrieving the user list. Please try again later.");
            }
        }

        // Action để xem chi tiết người dùng
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details action called with null ID.");
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {Id} not found.", id);
                    return NotFound();
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles
                };

                return View(userViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving details for user with ID {Id}.", id);
                return StatusCode(500, "An error occurred while retrieving user details. Please try again later.");
            }
        }
    }
}