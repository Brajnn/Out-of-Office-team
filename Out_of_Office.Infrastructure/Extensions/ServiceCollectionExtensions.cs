using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Out_of_Office.Domain.Interfaces;
using Out_of_Office.Infrastructure.Presistance;
using Out_of_Office.Infrastructure.Repositories;
using Out_of_Office.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Out_of_Office.Infrastructure.Identity;
using Out_of_Office.Application.Common.Interfaces;
using Out_of_Office.Infrastructure.Email;

namespace Out_of_Office.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Out_of_OfficeDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("OutOfOfficeConnectionString")));
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                //options.SignIn.RequireConfirmedEmail = true; // future
                options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            })
                .AddEntityFrameworkStores<Out_of_OfficeDbContext>()
                .AddDefaultTokenProviders();
            services.Configure<EmailOptions>(configuration.GetSection("Email"));

            services.AddScoped<IEmailSender>(sp =>
            {
                var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailOptions>>().Value;
                return opt.UseFilePickup
                    ? new FileEmailSender(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailOptions>>())
                    : new SmtpEmailSender(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailOptions>>());
            });
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmployeeRepository,EmployeeRepository>();
            services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IEmployeeProjectRepository, EmployeeProjectRepository>();
            services.AddScoped<IWorkCalendarRepository, WorkCalendarRepository>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddHttpContextAccessor();

        }
    }
}
