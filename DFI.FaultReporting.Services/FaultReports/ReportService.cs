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

        public async Task<Report> GetReport(int ID, string token)
        {
            Report report = await _reportHttp.GetReport(ID, token);

            return report;
        }

        public async Task<Report> CreateReport(Report report, string token)
        {
            report = await _reportHttp.CreateReport(report, token);

            return report;
        }

        public async Task<Report> UpdateReport(Report report, string token)
        {
            report = await _reportHttp.UpdateReport(report, token);

            return report;
        }
    }
}
