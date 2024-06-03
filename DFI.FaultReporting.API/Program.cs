
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.SQL.Repository.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using DFI.FaultReporting.SQL.Repository.FaultReports;
using DFI.FaultReporting.SQL.Repository.Interfaces.Files;
using DFI.FaultReporting.SQL.Repository.Files;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.SQL.Repository.Roles;
using DFI.FaultReporting.SQL.Repository.Interfaces.Users;
using DFI.FaultReporting.SQL.Repository.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DFIFaultReportingDataContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddScoped<DFIFaultReportingDataContext, DFIFaultReportingDataContext>();

builder.Services.AddScoped<IClaimStatusSQLRepository, ClaimStatusSQLRepository>();
builder.Services.AddScoped<IClaimTypeSQLRepository, ClaimTypeSQLRepository>();
builder.Services.AddScoped<IFaultPrioritySQLRepository, FaultPrioritySQLRepository>();
builder.Services.AddScoped<IFaultStatusSQLRepository, FaultStatusSQLRepository>();
builder.Services.AddScoped<IFaultTypeSQLRepository, FaultTypeSQLRepository>();
builder.Services.AddScoped<IFaultSQLRepository, FaultSQLRepository>();
builder.Services.AddScoped<IReportSQLRepository, ReportSQLRepository>();
builder.Services.AddScoped<IReportPhotoSQLRepository, ReportPhotoSQLRepository>();
builder.Services.AddScoped<IRoleSQLRepository, RoleSQLRepository>();
builder.Services.AddScoped<IUserSQLRepository, UserSQLRepository>();
builder.Services.AddScoped<IUserRoleSQLRepository, UserRoleSQLRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
