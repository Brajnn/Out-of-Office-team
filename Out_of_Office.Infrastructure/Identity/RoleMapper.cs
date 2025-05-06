using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Infrastructure.Identity
{
    public static class RoleMapper
    {
        public static string MapPositionToRole(string position)
        {
            return position switch
            {
                "Employee" => "Employee",
                "HR Manager" => "HRManager",
                "Project Manager" => "ProjectManager",
                "Administrator" => "Administrator",
                _ => throw new ArgumentException($"Wrong position: {position}")
            };
        }

        public static List<string> GetAllDisplayPositions()
        {
            return new List<string>
            {
                "Employee",
                "HR Manager",
                "Project Manager",
                "Administrator"
            };
        }
    }
}
