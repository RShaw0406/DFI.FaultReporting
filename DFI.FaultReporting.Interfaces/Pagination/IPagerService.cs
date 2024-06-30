using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DFI.FaultReporting.Services.Interfaces.Pagination
{
    public interface IPagerService
    {
        //Declare GetPaginatedFaults method, this is needed for paginating the faults.
        Task<List<Fault>> GetPaginatedFaults(List<Fault> faults, int currentPage, int pageSize);

        //Declare GetPaginatedReports method, this is needed for paginating the reports.
        Task<List<Report>> GetPaginatedReports(List<Report> reports, int currentPage, int pageSize);
    }
}
