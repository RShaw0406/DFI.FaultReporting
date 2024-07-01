using DFI.FaultReporting.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Roles
{
    public interface IStaffRoleSQLRepository
    {
        Task<List<StaffRole>> GetStaffRoles();

        Task<StaffRole> GetStaffRole(int ID);

        Task<StaffRole> CreateStaffRole(StaffRole staffRole);

        Task<StaffRole> UpdateStaffRole(StaffRole staffRole);

        Task<int> DeleteStaffRole(int ID);
    }
}
