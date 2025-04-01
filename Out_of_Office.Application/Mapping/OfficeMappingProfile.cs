using AutoMapper;
using Out_of_Office.Application.Approval_Request;
using Out_of_Office.Application.Employee;
using Out_of_Office.Application.Leave_Balance;
using Out_of_Office.Application.Leave_Request;
using Out_of_Office.Application.Project;
using Out_of_Office.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<LeaveRequest, LeaveRequestDto>()
           .ForMember(dest => dest.EmployeeFullName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.SubmittedAt, opt => opt.MapFrom(src => src.SubmittedAt))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<LeaveBalance, LeaveBalanceDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<Domain.Entities.Project, ProjectDto>();

 

        }
    }
}
