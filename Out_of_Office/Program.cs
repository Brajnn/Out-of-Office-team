using Microsoft.EntityFrameworkCore;
using Out_of_Office.Infrastructure.Presistance;
using Out_of_Office.Infrastructure.Extensions;
using Out_of_Office.Application.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews(options =>
options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAplication();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/accessdenied";
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
    options.AddPolicy("HRManagerOnly", policy => policy.RequireRole("HRManager"));
    options.AddPolicy("ProjectManagerOnly", policy => policy.RequireRole("Project Manager"));
    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "lists-employees",
    pattern: "/Lists/Employees",
    defaults: new { controller = "Employee", action = "Index" }
);
app.MapControllerRoute(
    name: "lists-projects",
    pattern: "/Lists/Projects",
    defaults: new { controller = "Project", action = "Index" }
);
app.MapControllerRoute(
    name: "lists-leaverequest",
    pattern: "/Lists/LeaveRequests",
    defaults: new { controller = "LeaveRequest", action = "Index" }
);
app.MapControllerRoute(
    name: "lists-ApprovalRequest",
    pattern: "/Lists/ApprovalRequests",
    defaults: new { controller = "ApprovalRequest", action = "Index" }
);
app.MapControllerRoute(
    name: "auth",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
