using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.FaultReports
{
    public class ReportSQLRepository : IReportSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ReportSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Report>? Reports { get; set; }

        public async Task<List<Report>> GetReports()
        {
            Reports = await _context.Report.ToListAsync();
            return Reports;
        }

        public async Task<Report> GetReport(int ID)
        {
            Report report = await _context.Report.FindAsync(ID);
            return report;
        }

        public async Task<Report> CreateReport(Report report)
        {
            _context.Report.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> UpdateReport(Report report)
        {
            _context.Entry(report).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<int> DeleteReport(int ID)
        {
            Report report = await _context.Report.Where(cs => cs.ID == ID).FirstOrDefaultAsync();
            _context.Report.Remove(report);
            await _context.SaveChangesAsync();
            return ID;
        }
    }
}
