using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Http.Files;
using DFI.FaultReporting.Http.Roles;
using DFI.FaultReporting.Http.Users;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Files;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Roles;
using DFI.FaultReporting.Services.Settings;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddHttpClient("API", api =>
{
    api.BaseAddress = new Uri(builder.Configuration.GetValue<string>("API:BaseURL"));
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IClaimStatusService, ClaimStatusService>();
builder.Services.AddScoped<IClaimTypeService, ClaimTypeService>();
builder.Services.AddScoped<IFaultPriorityService, FaultPriorityService>();
builder.Services.AddScoped<IFaultStatusService, FaultStatusService>();
builder.Services.AddScoped<IFaultTypeService, FaultTypeService>();
builder.Services.AddScoped<IFaultService, FaultService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportPhotoService, ReportPhotoService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();

builder.Services.AddScoped<ClaimStatusHttp, ClaimStatusHttp>();
builder.Services.AddScoped<ClaimTypeHttp, ClaimTypeHttp>();
builder.Services.AddScoped<FaultPriorityHttp, FaultPriorityHttp>();
builder.Services.AddScoped<FaultStatusHttp, FaultStatusHttp>();
builder.Services.AddScoped<FaultTypeHttp, FaultTypeHttp>();
builder.Services.AddScoped<FaultHttp, FaultHttp>();
builder.Services.AddScoped<ReportHttp, ReportHttp>();
builder.Services.AddScoped<ReportPhotoHttp, ReportPhotoHttp>();
builder.Services.AddScoped<RoleHttp, RoleHttp>();
builder.Services.AddScoped<UserHttp, UserHttp>();
builder.Services.AddScoped<UserRoleHttp, UserRoleHttp>();

builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.AccessDeniedPath = "/Error/";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
