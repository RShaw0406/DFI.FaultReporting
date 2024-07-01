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

        public async Task<List<Role>> GetRoles(string token)
        {
            Roles = await _roleHttp.GetRoles(token);

            return Roles;
        }

        public async Task<Role> GetRole(int ID, string token)
        {
            Role role = await _roleHttp.GetRole(ID, token);

            return role;
        }

        public async Task<Role> CreateRole(Role role, string token)
        {
            role = await _roleHttp.CreateRole(role, token);

            return role;
        }

        public async Task<Role> UpdateRole(Role role, string token)
        {
            role = await _roleHttp.UpdateRole(role, token);

            return role;
        }

        public async Task<int> DeleteRole(int ID, string token)
        {
            await _roleHttp.DeleteRole(ID, token);

            return ID;
        }
    }
}
