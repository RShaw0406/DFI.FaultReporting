using DFI.FaultReporting.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Roles
{
    public interface IStaffRoleService
    {
        Task<List<StaffRole>> GetStaffRoles(string token);

        Task<StaffRole> GetStaffRole(int ID, string token);

        Task<StaffRole> CreateStaffRole(StaffRole staffRole, string token);

        Task<StaffRole> UpdateStaffRole(StaffRole staffRole, string token);

        Task<int> DeleteStaffRole(int ID, string token);
    }
}
