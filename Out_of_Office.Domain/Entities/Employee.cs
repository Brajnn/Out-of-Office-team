using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Entities
{
    public class Employee
    {
        public int Id { get; set; }  
        public string FullName { get; set; }

        public string Subdivision { get; set; }

        public string Position { get; set; }

        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }

        [EnumDataType(typeof(EmployeeStatus))]
        public EmployeeStatus Status { get; set; }
        public int PeoplePartnerID { get; set; }
        public int OutOfOfficeBalance { get; set; }
        public byte[] Photo { get; set; }
        public ICollection<EmployeeProject> EmployeeProjects { get; set; }
        public ICollection<LeaveBalance> LeaveBalances { get; set; }
    }

    public enum EmployeeStatus
    {
        Active,
        Inactive
    }
}
