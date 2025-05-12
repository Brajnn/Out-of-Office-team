using Microsoft.EntityFrameworkCore;
using Out_of_Office.Infrastructure.Presistance;
using Out_of_Office.Infrastructure.Extensions;
using Out_of_Office.Application.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Out_of_Office.Infrastructure.Identity;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews(options =>
options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
builder.Services.AddSession();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAplication();
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
app.UseSession();

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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentityInitializer.SeedRolesAsync(roleManager);
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var dbContext = services.GetRequiredService<Out_of_OfficeDbContext>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    await IdentityInitializer.SeedRolesAsync(roleManager);
    await IdentityInitializer.SeedAdminAsync(userManager, roleManager, dbContext);
    await IdentityInitializer.SeedDemoUsersAsync(userManager, roleManager, dbContext);
}
app.Run();
