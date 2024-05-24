
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.SQL.Repository.Admin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DFIFaultReportingDataContext>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddScoped<DFIFaultReportingDataContext, DFIFaultReportingDataContext>();

builder.Services.AddScoped<IClaimStatusSQLRepository, ClaimStatusSQLRepository>();
builder.Services.AddScoped<IClaimTypeSQLRepository, ClaimTypeSQLRepository>();
builder.Services.AddScoped<IFaultStatusSQLRepository, FaultStatusSQLRepository>();
builder.Services.AddScoped<IFaultTypeSQLRepository, FaultTypeSQLRepository>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
