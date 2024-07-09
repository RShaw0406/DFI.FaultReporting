using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
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

        //Declare GetPaginatedStaff method, this is needed for paginating the staff.
        Task<List<Staff>> GetPaginatedStaff(List<Staff> staff, int currentPage, int pageSize);

        //Declare GetPaginatedClaimStatuses method, this is needed for paginating the claim statuses.
        Task<List<ClaimStatus>> GetPaginatedClaimStatuses(List<ClaimStatus> claimStatuses, int currentPage, int pageSize);

        //Declare GetPaginatedClaimTypes method, this is needed for paginating the claim types.
        Task<List<ClaimType>> GetPaginatedClaimTypes(List<ClaimType> claimTypes, int currentPage, int pageSize);

        //Declare GetPaginatedFaultStatuses method, this is needed for paginating the fault statuses.
        Task<List<FaultStatus>> GetPaginatedFaultStatuses(List<FaultStatus> faultStatuses, int currentPage, int pageSize);

        //Declare GetPaginatedFaultTypes method, this is needed for paginating the fault types.
        Task<List<FaultType>> GetPaginatedFaultTypes(List<FaultType> faultTypes, int currentPage, int pageSize);

        //Declare GetPaginatedFaultPriorities method, this is needed for paginating the fault priorities.
        Task<List<FaultPriority>> GetPaginatedFaultPriorities(List<FaultPriority> faultPriorities, int currentPage, int pageSize);

        //Declare GetPaginatedRoles method, this is needed for paginating the roles.
        Task<List<Role>> GetPaginatedRoles(List<Role> roles, int currentPage, int pageSize);

        //Declare GetPaginatedContractors method, this is needed for paginating the contractors.
        Task<List<Contractor>> GetPaginatedContractors(List<Contractor> contractors, int currentPage, int pageSize);

        //Declare GetPaginatedRepairStatuses method, this is needed for paginating the repair statuses.
        Task<List<RepairStatus>> GetPaginatedRepairStatuses(List<RepairStatus> repairStatuses, int currentPage, int pageSize);

        //Declare GetPaginatedRepairs method, this is needed for paginating the repairs.
        Task<List<Repair>> GetPaginatedRepairs(List<Repair> repairs, int currentPage, int pageSize);
    }
}
