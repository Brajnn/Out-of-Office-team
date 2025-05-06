using Microsoft.AspNetCore.Identity;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Infrastructure.Presistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Identity
{
    public static class IdentityInitializer
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Employee", "HRManager", "ProjectManager", "Administrator" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, Out_of_OfficeDbContext dbContext)
        {
            var adminUsername = "admin125";
            var adminEmail = "admin@office.local";

            // 1. Sprawdź, czy użytkownik już istnieje
            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser != null)
            {
                // upewnij się, że ma przypisaną rolę
                if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
                return;
            }

            // 2. Jeśli nie ma użytkownika, to dodaj również pracownika
            var adminEmployee = new Employee
            {
                FullName = "Admin Systemowy",
                Subdivision = "IT",
                Position = "Administrator",
                HireDate = new DateTime(2023, 1, 1),
                Status = EmployeeStatus.Active,
                PeoplePartnerID = 0,
                OutOfOfficeBalance = 30,
                Photo = null,
                LeaveBalances = new List<LeaveBalance>
        {
            new LeaveBalance { Type = LeaveType.Vacation, DaysAvailable = 20 },
            new LeaveBalance { Type = LeaveType.SickLeave, DaysAvailable = 10 },
            new LeaveBalance { Type = LeaveType.Unpaid, DaysAvailable = 0 }
        }
            };

            dbContext.Employees.Add(adminEmployee);
            await dbContext.SaveChangesAsync();

            adminUser = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true,
                EmployeeId = adminEmployee.Id
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (!result.Succeeded)
            {
                throw new Exception("❌ Nie udało się utworzyć konta admina: " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
        public static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, Out_of_OfficeDbContext dbContext)
        {
            async Task CreateUser(string username, string email, string role, string fullName)
            {
                if (await userManager.FindByNameAsync(username) != null) return;

                var employee = new Employee
                {
                    FullName = fullName,
                    Subdivision = "IT",
                    Position = role,
                    HireDate = new DateTime(2023, 1, 1),
                    Status = EmployeeStatus.Active,
                    PeoplePartnerID = 1,
                    OutOfOfficeBalance = 20,
                    LeaveBalances = new List<LeaveBalance>
            {
                new LeaveBalance { Type = LeaveType.Vacation, DaysAvailable = 15 },
                new LeaveBalance { Type = LeaveType.SickLeave, DaysAvailable = 5 },
                new LeaveBalance { Type = LeaveType.Unpaid, DaysAvailable = 0 }
            }
                };
                dbContext.Employees.Add(employee);
                await dbContext.SaveChangesAsync();

                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    EmployeeId = employee.Id
                };

                var result = await userManager.CreateAsync(user, "Test123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            await CreateUser("hr", "hr@office.local", "HRManager", "Helena HR");
            await CreateUser("user", "user@office.local", "Employee", "Jan Kowalski");
        }
    }
}
