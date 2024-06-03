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
    public class UserRoleService : IUserRoleService
    {
        private readonly UserRoleHttp _userRoleHttp;

        public List<UserRole>? UserRoles { get; set; }

        public UserRoleService(UserRoleHttp userRoleHttp)
        {
            _userRoleHttp = userRoleHttp;
        }

        public async Task<List<UserRole>> GetUserRoles()
        {
            UserRoles = await _userRoleHttp.GetUserRoles();

            return UserRoles;
        }

        public async Task<UserRole> GetUserRole(int ID)
        {
            UserRole userRole = await _userRoleHttp.GetUserRole(ID);

            return userRole;
        }

        public async Task<UserRole> CreateUserRole(UserRole userRole)
        {
            userRole = await _userRoleHttp.CreateUserRole(userRole);

            return userRole;
        }

        public async Task<UserRole> UpdateUserRole(UserRole userRole)
        {
            userRole = await _userRoleHttp.UpdateUserRole(userRole);

            return userRole;
        }

        public async Task<int> DeleteUserRole(int ID)
        {
            await _userRoleHttp.DeleteUserRole(ID);

            return ID;
        }
    }
}
