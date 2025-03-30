using Out_of_Office.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Interfaces
{
    public interface IWorkCalendarRepository
    {
        Task<bool> ExistsAsync(int year);
        Task AddCalendarAsync(int year, List<WorkCalendarDay> days);
        Task<List<int>> GetAvailableYearsAsync();
        Task<List<WorkCalendarDay>> GetByYearAsync(int year);
    }
}
