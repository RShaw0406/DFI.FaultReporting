using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Http.Claims;
using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Http.Files;
using DFI.FaultReporting.Http.Roles;
using DFI.FaultReporting.Http.Users;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.Claims;
using DFI.FaultReporting.Services.Emails;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Files;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Claims;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Passwords;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Pagination;
using DFI.FaultReporting.Services.Passwords;
using DFI.FaultReporting.Services.Roles;
using DFI.FaultReporting.Services.Settings;
using DFI.FaultReporting.Services.Tokens;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Authentication.Cookies;

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
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IStaffRoleService, StaffRoleService>();
builder.Services.AddScoped<IContractorService, ContractorService>();
builder.Services.AddScoped<IRepairService, RepairService>();
builder.Services.AddScoped<IRepairPhotoService, RepairPhotoService>();
builder.Services.AddScoped<IRepairStatusService, RepairStatusService>();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IClaimFileService, ClaimFileService>();
builder.Services.AddScoped<IClaimPhotoService, ClaimPhotoService>();
builder.Services.AddScoped<ILegalRepService, LegalRepService>();
builder.Services.AddScoped<IWitnessService, WitnessService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ClaimStatusHttp, ClaimStatusHttp>();
builder.Services.AddScoped<ClaimTypeHttp, ClaimTypeHttp>();
builder.Services.AddScoped<FaultPriorityHttp, FaultPriorityHttp>();
builder.Services.AddScoped<FaultStatusHttp, FaultStatusHttp>();
builder.Services.AddScoped<FaultTypeHttp, FaultTypeHttp>();
builder.Services.AddScoped<FaultHttp, FaultHttp>();
builder.Services.AddScoped<ReportHttp, ReportHttp>();
builder.Services.AddScoped<ReportPhotoHttp, ReportPhotoHttp>();
builder.Services.AddScoped<RoleHttp, RoleHttp>();
builder.Services.AddScoped<StaffHttp, StaffHttp>();
builder.Services.AddScoped<StaffRoleHttp, StaffRoleHttp>();
builder.Services.AddScoped<ContractorHttp, ContractorHttp>();
builder.Services.AddScoped<RepairHttp, RepairHttp>();
builder.Services.AddScoped<RepairPhotoHttp, RepairPhotoHttp>();
builder.Services.AddScoped<RepairStatusHttp, RepairStatusHttp>();
builder.Services.AddScoped<ClaimHttp, ClaimHttp>();
builder.Services.AddScoped<ClaimFileHttp, ClaimFileHttp>();
builder.Services.AddScoped<ClaimPhotoHttp, ClaimPhotoHttp>();
builder.Services.AddScoped<LegalRepHttp, LegalRepHttp>();
builder.Services.AddScoped<WitnessHttp, WitnessHttp>();
builder.Services.AddScoped<UserHttp, UserHttp>();


builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IVerificationTokenService, VerificationTokenService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IMimeSnifferService, MimeSnifferService>();
builder.Services.AddScoped<IFileDetectorService, ImageFileDetectorService>();
builder.Services.AddScoped<IFileDetectorService, PDFFileDetectorService>();
builder.Services.AddScoped<IFileDetectorService, WordFileDetectorService>();
builder.Services.AddScoped<IFileValidationService, FileValidationService>();
builder.Services.AddScoped<IPagerService, PagerService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        //options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.AccessDeniedPath = "/Error/";
    });

//Added for session state
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
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
app.UseCookiePolicy();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();