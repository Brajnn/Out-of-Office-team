using AutoMapper;
using MediatR;
using Out_of_Office.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.Employee.Queries.GetEmployeeById
{
    public class GetEmployeeByIdQueryHandler: IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository, IMapper mapper, IUserService userService)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(request.Id);
            if (employee == null)
            {
                return null; 
            }
            var dto= _mapper.Map<EmployeeDto>(employee);
            dto.Username = await _userService.GetUsernameByEmployeeIdAsync(employee.Id);
            return dto;
        }
    }
}
