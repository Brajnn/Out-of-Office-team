using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Out_of_Office.Application.User.Query.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public int Id { get; set; }
    }
}
