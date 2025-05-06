using Microsoft.EntityFrameworkCore;
using Out_of_Office.Application.Leave_Balance;
using Out_of_Office.Domain.Entities;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Presistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly Out_of_OfficeDbContext _dbContext;

        public EmployeeRepository(Out_of_OfficeDbContext dbContext)
        {
            _dbContext=dbContext;
        }
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _dbContext.Employees.ToListAsync();
        }
        public async Task AddEmployeeAsync(Employee employee)
        {
            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<Employee> GetEmployeeByIdAsync(int Id)
        {
            return await _dbContext.Employees
                .Include(e => e.LeaveBalances)
                .FirstOrDefaultAsync(e => e.Id == Id);
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            _dbContext.Employees.Update(employee);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<IList<Employee>> GetProjectManagersAsync()
        {
            return await _dbContext.Employees
                                 .Where(e => e.Position == "Project Manager")
                                 .ToListAsync();
        }
        public async Task UpdateLeaveBalancesAsync(int employeeId, List<(LeaveType Type, int DaysAvailable)> balances)
        {
            var existing = await _dbContext.LeaveBalances
                .Where(lb => lb.EmployeeId == employeeId)
                .ToListAsync();

            foreach (var (type, days) in balances)
            {
                Console.WriteLine($"UPDATE/ADD: {type} = {days}");
                var current = existing.FirstOrDefault(lb => lb.Type == type);
                if (current != null)
                {
                    current.DaysAvailable = days;
                }
                else
                {
                    _dbContext.LeaveBalances.Add(new LeaveBalance
                    {
                        EmployeeId = employeeId,
                        Type = type,
                        DaysAvailable = days
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
        }
        public async Task<List<Employee>> GetEmployeesWithoutAccountAsync()
        {
            return await _dbContext.Employees
                .Where(e => !_dbContext.Users.Any(u => u.EmployeeId == e.Id))
                .ToListAsync();
        }
        public async Task<Employee?> GetEmployeeByApplicationUserIdAsync(string applicationUserId)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == applicationUserId);

            if (user == null || user.EmployeeId == null)
                return null;

            return await _dbContext.Employees
                .FirstOrDefaultAsync(e => e.Id == user.EmployeeId);
        }
    }
}
