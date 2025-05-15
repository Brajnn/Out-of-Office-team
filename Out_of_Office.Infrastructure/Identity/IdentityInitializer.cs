using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
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

            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Administrator"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
                return;
            }
            var projectManagerEmployee = dbContext.Employees
                    .FirstOrDefault(e => e.Position == "ProjectManager");
            var adminEmployee = new Employee
            {
                FullName = "Admin Systemowy",
                Subdivision = "IT",
                Position = "Administrator",
                HireDate = new DateTime(2023, 1, 1),
                Status = EmployeeStatus.Active,
                PeoplePartnerID = projectManagerEmployee.Id,
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
                throw new Exception("Can't create an admin account: " +
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
            await CreateUser("pm", "pm@office.local", "ProjectManager", "Piotr Manager");
            await CreateUser("hr", "hr@office.local", "HRManager", "Helena HR");
            await CreateUser("user", "user@office.local", "Employee", "Jan Kowalski");
        }
        public static async Task SeedExampleProjectAndCalendarAsync(UserManager<ApplicationUser> userManager, IMediator mediator, Out_of_OfficeDbContext dbContext)
        {

            // Project seeding
            if (!dbContext.Projects.Any())
            {
                var pmUser = await userManager.FindByNameAsync("pm");
                if (pmUser?.EmployeeId == null)
                {
                    Console.WriteLine("Skipping project seeding: Project Manager user does not have an associated Employee ID.");
                    return;
                }
                var project = new Project
                {
                    ProjectType = "Internal",
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddDays(60),
                    ProjectManagerID = pmUser.EmployeeId.Value,
                    Comment = "Demo project for showcasing the system",
                    Status = ProjectStatus.Active,
                };

                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();


                var employeeIds = dbContext.Employees.Select(e => e.Id).ToList();
                foreach (var employeeId in employeeIds)
                {
                    await mediator.Send(new Out_of_Office.Application.Project.Command.AssignEmployee.AssignEmployeeCommand
                    {
                        EmployeeId = employeeId,
                        ProjectId = project.ID
                    });
                }
            }

            // Calendar seeding
            if (!dbContext.WorkCalendarDays.Any())
            {
                int year = DateTime.UtcNow.Year;
                var daysInYear = Enumerable.Range(1, 12)
                    .SelectMany(month => Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                    .Select(day => new DateTime(year, month, day)));

                foreach (var day in daysInYear)
                {
                    dbContext.WorkCalendarDays.Add(new WorkCalendarDay
                    {
                        Year = year,
                        Date = day,
                        IsHoliday = day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday,
                        Description = day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday
                            ? "Weekend"
                            : "Working day"
                    });
                }

                await dbContext.SaveChangesAsync();
            }
        }

    }
}
