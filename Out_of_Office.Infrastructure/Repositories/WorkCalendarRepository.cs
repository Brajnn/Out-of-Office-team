using Microsoft.EntityFrameworkCore;
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
    public class WorkCalendarRepository : IWorkCalendarRepository
    {
        private readonly Out_of_OfficeDbContext _dbContext;

        public WorkCalendarRepository(Out_of_OfficeDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task<bool> ExistsAsync(int year)
        {
            return await _dbContext.WorkCalendarDays.AnyAsync(d => d.Year == year);
        }

        public async Task AddCalendarAsync(int year, List<WorkCalendarDay> days)
        {
            foreach (var day in days)
            {
                day.Year = year; 
            }

            await _dbContext.WorkCalendarDays.AddRangeAsync(days);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            return await _dbContext.WorkCalendarDays
                .Select(d => d.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
        }
        public async Task<List<WorkCalendarDay>> GetByYearAsync(int year)
        {
            return await _dbContext.WorkCalendarDays
                .Where(d => d.Year == year)
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

    }

}
