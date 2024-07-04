using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
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

        //Method to get paginated staff.
        public async Task<List<Staff>> GetPaginatedStaff(List<Staff> staff, int currentPage, int pageSize)
        {
            return staff.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated claim statuses.
        public async Task<List<ClaimStatus>> GetPaginatedClaimStatuses(List<ClaimStatus> claimStatuses, int currentPage, int pageSize)
        {
            return claimStatuses.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated claim types.
        public async Task<List<ClaimType>> GetPaginatedClaimTypes(List<ClaimType> claimTypes, int currentPage, int pageSize)
        {
            return claimTypes.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated fault statuses.
        public async Task<List<FaultStatus>> GetPaginatedFaultStatuses(List<FaultStatus> faultStatuses, int currentPage, int pageSize)
        {
            return faultStatuses.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated fault types.
        public async Task<List<FaultType>> GetPaginatedFaultTypes(List<FaultType> faultTypes, int currentPage, int pageSize)
        {
            return faultTypes.OrderByDescending(f => f.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
