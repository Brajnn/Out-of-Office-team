using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Leave_Request.Errors;

public enum LeaveRequestError
{
    VacationAdvanceError,
    UnpaidAdvanceError,
    SickLeavePastError,
    InvalidAbsenceReason,
    EndDateEarlierThanStart,
    EmployeeNotFound,
    NoCalendarForYear,
    InsufficientLeaveDays,
    OverlappingRequest
}
public class LeaveRequestValidationException : Exception
{
    public LeaveRequestError ErrorCode { get; }

    public LeaveRequestValidationException(LeaveRequestError errorCode)
    {
        ErrorCode = errorCode;
    }
}