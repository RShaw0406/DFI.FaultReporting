using DFI.FaultReporting.Http.Roles;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Services.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Roles
{
    public class StaffRoleService : IStaffRoleService
    {
        private readonly StaffRoleHttp _staffRoleHttp;

        public List<StaffRole>? StaffRoles { get; set; }

        public StaffRoleService(StaffRoleHttp staffRoleHttp)
        {
            _staffRoleHttp = staffRoleHttp;
        }

        public async Task<List<StaffRole>> GetStaffRoles(string token)
        {
            StaffRoles = await _staffRoleHttp.GetStaffRoles(token);

            return StaffRoles;
        }

        public async Task<StaffRole> GetStaffRole(int ID, string token)
        {
            StaffRole staffRole = await _staffRoleHttp.GetStaffRole(ID, token);

            return staffRole;
        }

        public async Task<StaffRole> CreateStaffRole(StaffRole staffRole, string token)
        {
            staffRole = await _staffRoleHttp.CreateStaffRole(staffRole, token);

            return staffRole;
        }

        public async Task<StaffRole> UpdateStaffRole(StaffRole staffRole, string token)
        {
            staffRole = await _staffRoleHttp.UpdateStaffRole(staffRole, token);

            return staffRole;
        }

        public async Task<int> DeleteStaffRole(int ID, string token)
        {
            await _staffRoleHttp.DeleteStaffRole(ID, token);

            return ID;
        }
    }
}
