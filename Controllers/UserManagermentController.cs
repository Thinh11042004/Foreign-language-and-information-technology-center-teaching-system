using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Enums;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Identity;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Users;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.User;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.ViewModel.UserViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class UserManagementController : BaseController
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UserManagementController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
                                       RoleManager<ApplicationRole> roleManager, ILogger<UserManagementController> logger)
            : base(context, userManager, logger)
        {
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchString, int page = 1, int pageSize = 10)
        {
            var users = _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.UserName.Contains(searchString) || u.Email.Contains(searchString));
            }

            var count = await users.CountAsync();
            var items = await users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new UserListViewModel
            {
                Users = items,
                SearchString = searchString,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize),
                Roles = await _roleManager.Roles.ToListAsync()  
            };

            return View(model);
        }


        public async Task<IActionResult> Users(int page = 1, string search = "", string role = "")
        {
            var query = _context.Users.Include(u => u.UserRoles).ThenInclude(r => r.Role).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role);
                var userIds = usersInRole.Select(u => u.Id);
                query = query.Where(u => userIds.Contains(u.Id));
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            var model = new UserListViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(await query.CountAsync() / 20.0),
                SearchTerm = search,
                SelectedRole = role,
                Roles = await _roleManager.Roles.ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var model = new CreateUserViewModel
            {
                Roles = await _roleManager.Roles.ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await _userManager.AddToRoleAsync(user, model.Role);
                    }

                    if (model.Role == "Teacher")
                    {
                        var teacher = new Teachers
                        {
                            UserId = user.Id,
                            EmployeeCode = GenerateEmployeeCode(),
                            JoinDate = DateTime.UtcNow,
                            IsAvailable = true
                        };
                        _context.Teachers.Add(teacher);
                    }
                    else if (model.Role == "Student")
                    {
                        var student = new Students
                        {
                            UserId = user.Id,
                            StudentCode = GenerateStudentCode(),
                            EnrollmentDate = DateTime.UtcNow,
                            Status = UserStatus.Active
                        };
                        _context.Students.Add(student);
                    }

                    await _context.SaveChangesAsync();
                    await LogActivityAsync("CreateUser", "User", user.Id);

                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.Roles = await _roleManager.Roles.ToListAsync();
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(role))
                return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                TempData["Error"] = "Người dùng đã có vai trò, không thể thay đổi.";
                return RedirectToAction("Index");
            }

            if (role == "SuperAdmin")
            {
                TempData["Error"] = "Không thể gán vai trò SuperAdmin.";
                return RedirectToAction("Index");
            }

            var result = await _userManager.AddToRoleAsync(user, role);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync(); 
                TempData["Success"] = "Vai trò đã được gán thành công.";
            }
            else
            {
                TempData["Error"] = "Gán vai trò thất bại.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] StatusUpdateRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return NotFound();

            if (Enum.TryParse<UserStatus>(request.NewStatus, out var status))
            {
                user.Status = status;
                await _userManager.UpdateAsync(user);
                return Ok();
            }

            return BadRequest("Invalid status");
        }





        private string GenerateStudentCode()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var sequence = _context.Students.Count() + 1;
            return $"ST{year}{sequence:D4}";
        }

        private string GenerateEmployeeCode()
        {
            var year = DateTime.Now.Year.ToString().Substring(2);
            var sequence = _context.Teachers.Count() + 1;
            return $"TC{year}{sequence:D4}";
        }
    }
}
