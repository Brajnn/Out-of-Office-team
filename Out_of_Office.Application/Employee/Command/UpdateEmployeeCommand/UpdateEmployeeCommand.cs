using MediatR;
using Out_of_Office.Application.Leave_Balance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Employee.Command.UpdateEmployeeCommand
{
    public class UpdateEmployeeCommand: IRequest
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Subdivision { get; set; }
        public string Position { get; set; }
        public DateTime HireDate { get; set; }
        public string Status { get; set; }
        public int PeoplePartnerID { get; set; }
        public int OutOfOfficeBalance { get; set; }
        public byte[] Photo { get; set; }
        public List<LeaveBalanceDto> LeaveBalances { get; set; }
    }
}
