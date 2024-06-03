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
        Task<List<User>> GetUsers();

        Task<User> GetUser(int ID);

        Task<User> CreateUser(User user);

        Task<User> UpdateUser(User user);

        Task<int> DeleteUser(int ID);
    }
}
