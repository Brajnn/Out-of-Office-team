using AutoMapper;
using Out_of_Office.Application.Approval_Request;
using Out_of_Office.Application.Employee;
using Out_of_Office.Application.Leave_Balance;
using Out_of_Office.Application.Leave_Request;
using Out_of_Office.Application.Project;
using Out_of_Office.Domain.Entities;

namespace Out_of_Office.Application.Mapping
{
    public class OfficeMappingProfile: Profile
    {
        public OfficeMappingProfile()
        {
            CreateMap<Domain.Entities.Employee, EmployeeDto>()
                .ForMember(dest => dest.LeaveBalances, opt => opt.MapFrom(src => src.LeaveBalances));

            CreateMap<ApprovalRequest, ApprovalRequestDto>()
           .ForMember(dest => dest.ApproverFullName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : string.Empty))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
           .ForMember(dest => dest.LeaveRequestSubmittedAt, opt => opt.MapFrom(src => src.LeaveRequest.SubmittedAt))
           .ForMember(dest => dest.ApproverID, opt => opt.MapFrom(src => src.Approver.Id))
           .ForMember(dest => dest.EmployeeFullName, opt => opt.MapFrom(src => src.LeaveRequest.Employee.FullName))
           .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.LeaveRequest.StartDate))
           .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.LeaveRequest.EndDate))
           .ForMember(dest => dest.LeaveRequestComment, opt => opt.MapFrom(src => src.LeaveRequest.Comment));

            CreateMap<LeaveRequest, LeaveRequestDto>()
           .ForMember(dest => dest.EmployeeFullName, opt => opt.MapFrom(src => src.Employee.FullName))
           .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.SubmittedAt))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
           
            CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<Domain.Entities.Project, ProjectDto>();



        }
    }
}
