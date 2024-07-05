using DFI.FaultReporting.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Roles
{
    public interface IRoleSQLRepository
    {
        Task<List<Role>> GetRoles();

        Task<Role> GetRole(int ID);

        Task<Role> CreateRole(Role role);

        Task<Role> UpdateRole(Role role);
    }
}
