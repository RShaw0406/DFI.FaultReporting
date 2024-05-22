using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Services.Admin;
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

builder.Services.AddScoped<ClaimStatusHttp, ClaimStatusHttp>();

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
