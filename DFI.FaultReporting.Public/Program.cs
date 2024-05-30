using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Settings;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddHttpClient("API", api =>
{
    api.BaseAddress = new Uri(builder.Configuration.GetValue<string>("API:BaseURL"));
});

builder.Services.AddScoped<IClaimStatusService, ClaimStatusService>();
builder.Services.AddScoped<IClaimTypeService, ClaimTypeService>();
builder.Services.AddScoped<IFaultPriorityService, FaultPriorityService>();
builder.Services.AddScoped<IFaultStatusService, FaultStatusService>();
builder.Services.AddScoped<IFaultTypeService, FaultTypeService>();
builder.Services.AddScoped<IFaultService, FaultService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<ClaimStatusHttp, ClaimStatusHttp>();
builder.Services.AddScoped<ClaimTypeHttp, ClaimTypeHttp>();
builder.Services.AddScoped<FaultPriorityHttp, FaultPriorityHttp>();
builder.Services.AddScoped<FaultStatusHttp, FaultStatusHttp>();
builder.Services.AddScoped<FaultTypeHttp, FaultTypeHttp>();
builder.Services.AddScoped<FaultHttp, FaultHttp>();
builder.Services.AddScoped<ReportHttp, ReportHttp>();

builder.Services.AddScoped<ISettingsService, SettingsService>();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
