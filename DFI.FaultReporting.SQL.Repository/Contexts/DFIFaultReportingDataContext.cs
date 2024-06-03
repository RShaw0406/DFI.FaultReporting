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
    }
}
