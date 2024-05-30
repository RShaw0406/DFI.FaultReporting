using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports
{
    public interface IReportSQLRepository
    {
        Task<List<Report>> GetReports();

        Task<Report> GetReport(int ID);

        Task<Report> CreateReport(Report report);

        Task<Report> UpdateReport(Report report);

        Task<int> DeleteReport(int ID);
    }
}
