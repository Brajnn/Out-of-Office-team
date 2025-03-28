using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Employee.Command.CreateEmployee
{
    public class CreateEmployeeCommand : IRequest
    {
        public string FullName { get; set; }
        public string Subdivision { get; set; }
        public string Position { get; set; }
        public string Status { get; set; }

        public DateTime HireDate { get; set; }
        public int PeoplePartnerID { get; set; }
        public int OutOfOfficeBalance { get; set; }
        public byte[] Photo { get; set; }

        public int VacationDays { get; set; }
        public int SickLeaveDays { get; set; }
        public int UnpaidLeaveDays { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
