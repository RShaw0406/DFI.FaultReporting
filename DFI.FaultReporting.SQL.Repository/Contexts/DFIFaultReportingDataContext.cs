using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Contexts
{
    public class DFIFaultReportingDataContext : DbContext
    {
        public DFIFaultReportingDataContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=tcp:dfifaultreportingsqlserver.database.windows.net,1433;Initial Catalog=DFIFaultReportingSQLDB;" +
            //    "Persist Security Info=False;User ID=DFIFaultReportingUser;Password=!!!Reporting!!!;" +
            //    "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DFI.FaultReporting;Trusted_Connection=True;MultipleActiveResultSets=true");
        }

        public DbSet<ClaimStatus> ClaimStatus { get; set; } = null!;

        public DbSet<ClaimType> ClaimType { get; set; } = null!;

        public DbSet<FaultStatus> FaultStatus { get; set; } = null!;

        public DbSet<FaultType> FaultType { get; set; } = null!;

        public DbSet<FaultPriority> FaultPriority { get; set; } = null!;

        public DbSet<Fault> Fault { get; set; } = null!;

        public DbSet<Report> Report { get; set; } = null!;

        public DbSet<ReportPhoto> ReportPhoto { get; set; } = null!;

        public DbSet<Role> Role { get; set; } = null!;

        public DbSet<User> User { get; set; } = null!;

        public DbSet<UserRole> UserRole { get; set; } = null!;

        public DbSet<Staff> Staff { get; set; } = null!;

        public DbSet<StaffRole> StaffRole { get; set; } = null!;

        public DbSet<Contractor> Contractor { get; set; } = null!;
    }
}
