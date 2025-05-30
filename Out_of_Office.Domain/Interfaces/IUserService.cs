﻿using Out_of_Office.Domain.Entities;
namespace Out_of_Office.Domain.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, IEnumerable<string> Errors)> CreateUserForEmployeeAsync(string username, string password, string role);
        Task<string> GetUsernameByEmployeeIdAsync(int employeeId);
        Task LinkUserToEmployeeAsync(string username, int employeeId);
    }
}
