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
    public class ProjectRepository :IProjectRepository
    {
        private readonly Out_of_OfficeDbContext _context;

        public ProjectRepository(Out_of_OfficeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                                 .Include(p => p.ProjectManager)
                                 .ToListAsync();
        }
        public async Task<Project> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
               .Include(p => p.ProjectManager)
               .Include(p => p.EmployeeProjects)
               .ThenInclude(ep => ep.Employee)
               .FirstOrDefaultAsync(p => p.ID == id);
        }
        public async Task<int> AddProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project.ID;
        }
        public async Task UpdateProjectAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }
        
    }
}
