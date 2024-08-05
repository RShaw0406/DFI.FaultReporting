using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.FaultReports
{
    public interface IReportService
    {
        Task<List<Report>> GetReports();

        Task<Report> GetReport(int ID, string token);

        Task<Report> CreateReport(Report report, string token);

        Task<Report> UpdateReport(Report report, string token);
    }
}
