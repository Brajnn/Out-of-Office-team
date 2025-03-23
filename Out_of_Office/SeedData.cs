using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Presistance;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<Out_of_OfficeDbContext>();
        var employeeRepo = serviceProvider.GetRequiredService<IEmployeeRepository>();
        var userService = serviceProvider.GetRequiredService<IUserService>();

        string adminUsername = "admin123";

        // Sprawdź, czy użytkownik już istnieje
        if (context.Users.Any(u => u.Username == adminUsername))
        {
            Console.WriteLine("ℹ️ Konto admin już istnieje.");
            return;
        }

        // Szukamy istniejącego pracownika bez przypisanego konta użytkownika
        var availableEmployee = context.Employee
            .FirstOrDefault(e => e.User == null && e.Position == "Administrator");

        // Jeśli nie ma, tworzymy nowego pracownika
        if (availableEmployee == null)
        {
            availableEmployee = new Employee
            {
                FullName = "Admin User",
                Subdivision = "IT",
                Position = "Admin",
                Status = EmployeeStatus.Active,
                PeoplePartnerID = 1016,
                OutOfOfficeBalance = 10,
                Photo = null
            };

            await employeeRepo.AddEmployeeAsync(availableEmployee);
        }

        // Tworzymy użytkownika powiązanego z pracownikiem
        await userService.RegisterAsync(
            username: adminUsername,
            password: "Test123!",
            employeeId: availableEmployee.Id,
            position: "Administrator"
        );

        Console.WriteLine("✅ Dodano użytkownika: admin / Test123!");
    }

}
