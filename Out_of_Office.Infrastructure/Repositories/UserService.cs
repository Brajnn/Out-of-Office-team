using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Repositories
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserForEmployeeAsync(string username, string password, Employee employee, string role)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = $"{username}@office.local",
                EmailConfirmed = true,
                Employee = employee
            };

            if (string.IsNullOrWhiteSpace(employee.Subdivision) ||
                string.IsNullOrWhiteSpace(employee.Position))
            {
                return (false, new[] { "Not enough data." });
            }

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(e => e.Description));
            }
            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                return (false, roleResult.Errors.Select(e => e.Description));
            }
            return (true, Enumerable.Empty<string>());

        }
        public async Task<string> GetUsernameByEmployeeIdAsync(int employeeId)
        {
            var user = await _userManager.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Employee != null && u.Employee.Id == employeeId);

            return user?.UserName;
        }
    }
}
