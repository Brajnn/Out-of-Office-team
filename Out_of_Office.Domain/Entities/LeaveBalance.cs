using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Domain.Entities
{
    public class LeaveBalance
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public LeaveType Type { get; set; }
        public int DaysAvailable { get; set; }
    }

    public enum LeaveType
    {
        Vacation,
        SickLeave,
        Unpaid
    }
}
