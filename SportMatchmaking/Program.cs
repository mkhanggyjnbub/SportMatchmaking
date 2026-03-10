using BusinessObjects;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;
using Repositories.AppUser;
using Services.AppUser;
using Services.Auth;
using Services.Admin;
using Repositories.Admin;   

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SportMatchmakingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("mvc_b1Context")));

builder.Services.AddScoped<AppUserDAO>();
builder.Services.AddScoped<EmailVerificationDAO>();

builder.Services.AddScoped<IAppUserRepository, AppUserRepository>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddScoped<AdminUserDAO>();
builder.Services.AddScoped<AdminPostDAO>();
builder.Services.AddScoped<AdminSportDAO>();
builder.Services.AddScoped<AdminReportDAO>();
builder.Services.AddScoped<AdminDashboardDAO>();

builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminPostRepository, AdminPostRepository>();
builder.Services.AddScoped<IAdminSportRepository, AdminSportRepository>();
builder.Services.AddScoped<IAdminReportRepository, AdminReportRepository>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();

builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminPostService, AdminPostService>();
builder.Services.AddScoped<IAdminSportService, AdminSportService>();
builder.Services.AddScoped<IAdminReportService, AdminReportService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
