using DFI.FaultReporting.Http.Roles;
using DFI.FaultReporting.Http.Users;
using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserHttp _userHttp;

        public List<User>? Users { get; set; }

        public UserService(UserHttp userHttp)
        {
            _userHttp = userHttp;
        }

        public async Task<AuthResponse> Login(LoginRequest loginRequest)
        {
            AuthResponse authResponse = await _userHttp.Login(loginRequest);

            return authResponse;
        }

        public async Task<List<User>> GetUsers(string token)
        {
            Users = await _userHttp.GetUsers(token);

            return Users;
        }

        public async Task<User> GetUser(int ID)
        {
            User user = await _userHttp.GetUser(ID);

            return user;
        }

        public async Task<User> CreateUser(User user)
        {
            user = await _userHttp.CreateUser(user);

            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            user = await _userHttp.UpdateUser(user);

            return user;
        }

        public async Task<int> DeleteUser(int ID)
        {
            await _userHttp.DeleteUser(ID);

            return ID;
        }
    }
}
