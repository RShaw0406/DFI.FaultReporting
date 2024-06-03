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
        Task<List<Role>> GetRoles();

        Task<Role> GetRole(int ID);

        Task<Role> CreateRole(Role role);

        Task<Role> UpdateRole(Role role);

        Task<int> DeleteRole(int ID);
    }
}
