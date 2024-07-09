using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Roles;
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
            return faults.OrderBy(f => f.FaultPriorityID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated reports.
        public async Task<List<Report>> GetPaginatedReports(List<Report> reports, int currentPage, int pageSize)
        {
            return reports.OrderByDescending(r => r.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated staff.
        public async Task<List<Staff>> GetPaginatedStaff(List<Staff> staff, int currentPage, int pageSize)
        {
            return staff.OrderByDescending(s => s.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated claim statuses.
        public async Task<List<ClaimStatus>> GetPaginatedClaimStatuses(List<ClaimStatus> claimStatuses, int currentPage, int pageSize)
        {
            return claimStatuses.OrderByDescending(cs => cs.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated claim types.
        public async Task<List<ClaimType>> GetPaginatedClaimTypes(List<ClaimType> claimTypes, int currentPage, int pageSize)
        {
            return claimTypes.OrderByDescending(ct => ct.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated fault statuses.
        public async Task<List<FaultStatus>> GetPaginatedFaultStatuses(List<FaultStatus> faultStatuses, int currentPage, int pageSize)
        {
            return faultStatuses.OrderByDescending(fs => fs.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated fault types.
        public async Task<List<FaultType>> GetPaginatedFaultTypes(List<FaultType> faultTypes, int currentPage, int pageSize)
        {
            return faultTypes.OrderByDescending(ft => ft.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated fault priorities.
        public async Task<List<FaultPriority>> GetPaginatedFaultPriorities(List<FaultPriority> faultPriorities, int currentPage, int pageSize)
        {
            return faultPriorities.OrderByDescending(fp => fp.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated roles.
        public async Task<List<Role>> GetPaginatedRoles(List<Role> roles, int currentPage, int pageSize)
        {
            return roles.OrderByDescending(r => r.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated contractors.
        public async Task<List<Contractor>> GetPaginatedContractors(List<Contractor> contractors, int currentPage, int pageSize)
        {
            return contractors.OrderByDescending(c => c.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated repair statuses.
        public async Task<List<RepairStatus>> GetPaginatedRepairStatuses(List<RepairStatus> repairStatuses, int currentPage, int pageSize)
        {
            return repairStatuses.OrderByDescending(rs => rs.ID).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }

        //Method to get paginated repairs.
        public async Task<List<Repair>> GetPaginatedRepairs(List<Repair> repairs, int currentPage, int pageSize)
        {
            return repairs.OrderBy(r => r.RepairTargetDate).Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        }
    }
}
