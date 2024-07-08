
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
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Tokens;
using Microsoft.Data.SqlClient;
using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Services.Interfaces.Passwords;
using DFI.FaultReporting.Services.Passwords;
//using DFI.FaultReporting.Services.Settings;

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
builder.Services.AddScoped<IStaffSQLRepository, StaffSQLRepository>();
builder.Services.AddScoped<IStaffRoleSQLRepository, StaffRoleSQLRepository>();
builder.Services.AddScoped<IContractorSQLRepository, ContractorSQLRepository>();
builder.Services.AddScoped<IRepairSQLRepository, RepairSQLRepository>();
builder.Services.AddScoped<IRepairPhotoSQLRepository, RepairPhotoSQLRepository>();
builder.Services.AddScoped<IRepairStatusSQLRepository, RepairStatusSQLRepository>();

builder.Services.AddScoped<DFI.FaultReporting.JWT.Requests.LoginRequest, DFI.FaultReporting.JWT.Requests.LoginRequest>();
builder.Services.AddScoped<RegistrationRequest, RegistrationRequest>();
builder.Services.AddScoped<AuthResponse, AuthResponse>();
builder.Services.AddScoped<IJWTTokenService, JWTTokenService>();

builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    // Define the OAuth2.0 scheme that's in use (i.e., Implicit Flow)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddRouting(options => options.LowercaseUrls = true);

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
