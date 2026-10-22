
using IncidentManagement.Data;
using IncidentManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Create roles and demo accounts only in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;

    var roleManager =
        services.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager =
        services.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "Employee", "Technician" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole(role));

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }

    var demoUsers = new[]
    {
        new
        {
            Email = "admin.demo@example.com",
            Password = "AdminDemo@123",
            FullName = "Demo Administrator",
            Role = "Admin"
        },
        new
        {
            Email = "employee.demo@example.com",
            Password = "EmployeeDemo@123",
            FullName = "Demo Employee",
            Role = "Employee"
        },
        new
        {
            Email = "technician.demo@example.com",
            Password = "TechDemo@123",
            FullName = "Demo Technician",
            Role = "Technician"
        }
    };

    foreach (var demo in demoUsers)
    {
        var user = await userManager.FindByEmailAsync(demo.Email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = demo.Email,
                Email = demo.Email,
                FullName = demo.FullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(
                user, demo.Password);

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, demo.Role))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user, demo.Role);

            if (!roleResult.Succeeded)
            {
                throw new Exception(
                    string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
