using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<AuthResponse> Register(RegistrationRequest registrationRequest);
        Task<AuthResponse> Login(JWT.Requests.LoginRequest loginRequest);

        Task<List<User>> GetUsers(string token);

        Task<User> GetUser(int ID, string token);

        Task<User> CreateUser(User user);

        Task<User> UpdateUser(User user, string token);

        Task<int> DeleteUser(int ID, string token);
    }
}
