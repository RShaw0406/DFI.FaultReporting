using DFI.FaultReporting.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Roles
{
    public interface IUserRoleService
    {
        Task<List<UserRole>> GetUserRoles();

        Task<UserRole> GetUserRole(int ID);

        Task<UserRole> CreateUserRole(UserRole userRole);

        Task<UserRole> UpdateUserRole(UserRole userRole);

        Task<int> DeleteUserRole(int ID);
    }
}
