using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Http.Roles;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Services.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Roles
{
    public class RoleService : IRoleService
    {
        private readonly RoleHttp _roleHttp;

        public List<Role>? Roles { get; set; }

        public RoleService(RoleHttp roleHttp)
        {
            _roleHttp = roleHttp;
        }

        public async Task<List<Role>> GetRoles()
        {
            Roles = await _roleHttp.GetRoles();

            return Roles;
        }

        public async Task<Role> GetRole(int ID)
        {
            Role role = await _roleHttp.GetRole(ID);

            return role;
        }

        public async Task<Role> CreateRole(Role role)
        {
            role = await _roleHttp.CreateRole(role);

            return role;
        }

        public async Task<Role> UpdateRole(Role role)
        {
            role = await _roleHttp.UpdateRole(role);

            return role;
        }

        public async Task<int> DeleteRole(int ID)
        {
            await _roleHttp.DeleteRole(ID);

            return ID;
        }
    }
}
