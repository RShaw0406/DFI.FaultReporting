using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.FaultReports
{
    public class ReportService : IReportService
    {
        private readonly ReportHttp _reportHttp;

        public List<Report>? Reports { get; set; }

        public ReportService(ReportHttp reportHttp)
        {
            _reportHttp = reportHttp;
        }

        public async Task<List<Report>> GetReports()
        {
            Reports = await _reportHttp.GetReports();

            return Reports;
        }

        public async Task<Report> GetReport(int ID)
        {
            Report report = await _reportHttp.GetReport(ID);

            return report;
        }

        public async Task<Report> CreateReport(Report report)
        {
            report = await _reportHttp.CreateReport(report);

            return report;
        }

        public async Task<Report> UpdateReport(Report report)
        {
            report = await _reportHttp.UpdateReport(report);

            return report;
        }

        public async Task<int> DeleteReport(int ID)
        {
            await _reportHttp.DeleteReport(ID);

            return ID;
        }
    }
}
