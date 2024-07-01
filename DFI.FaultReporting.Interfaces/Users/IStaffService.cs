using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Users
{
    public interface IStaffService
    {
        Task<AuthResponse> Login(JWT.Requests.LoginRequest loginRequest);
        Task<AuthResponse> Lock(string emailAddress);
        Task<List<Staff>> GetAllStaff(string token);

        Task<Staff> GetStaff(int ID, string token);

        Task<Staff> CreateStaff(Staff staff, string token);

        Task<Staff> UpdateStaff(Staff staff, string token);

        Task<int> DeleteStaff(int ID, string token);
    }
}
