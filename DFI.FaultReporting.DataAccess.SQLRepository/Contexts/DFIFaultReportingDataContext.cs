using DFI.FaultReporting.Models.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.DataAccess.SQL.Repository.Contexts
{
    public class DFIFaultReportingDataContext : DbContext
    {
        public DFIFaultReportingDataContext(DbContextOptions<DFIFaultReportingDataContext> options)
        : base(options)
        {
        }

        public DbSet<ClaimStatus> ClaimStatus { get; set; } = null!;
    }
}
