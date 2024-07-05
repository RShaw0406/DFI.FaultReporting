using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Roles
{
    public interface IRoleService
    {
        Task<List<Role>> GetRoles(string token);

        Task<Role> GetRole(int ID, string token);

        Task<Role> CreateRole(Role role, string token);

        Task<Role> UpdateRole(Role role, string token);
    }
}
