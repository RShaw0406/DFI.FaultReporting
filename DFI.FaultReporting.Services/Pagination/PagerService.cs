using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Pagination
{
    public class PagerService : IPagerService
    {
        //Method to get paginated faults.
        public async Task<List<Fault>> GetPaginatedFaults(List<Fault> faults, int currentPage, int pageSize)
        {
            return faults.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated reports.
        public async Task<List<Report>> GetPaginatedReports(List<Report> reports, int currentPage, int pageSize)
        {
            return reports.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
