using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Identity;
using Out_of_Office.Models;

namespace Out_of_Office.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmployeeRepository _employeeRepository;

        public UserManagementController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmployeeRepository employeeRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var employeesWithoutAccount = await _employeeRepository.GetEmployeesWithoutAccountAsync();
            ViewBag.Employees = employeesWithoutAccount;
            ViewBag.Roles = new[] { "Employee", "HRManager", "ProjectManager" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _employeeRepository.GetEmployeesWithoutAccountAsync();
                ViewBag.Roles = new[] { "Employee", "HRManager", "ProjectManager" };
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                EmployeeId = model.EmployeeId
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                ViewBag.Employees = await _employeeRepository.GetEmployeesWithoutAccountAsync();
                ViewBag.Roles = new[] { "Employee", "HRManager", "ProjectManager" };
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            return RedirectToAction("Index", "Employee");
        }
    }
}

