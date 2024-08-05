using DFI.FaultReporting.Http.Users;
using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Users
{
    public class StaffService : IStaffService
    {
        private readonly StaffHttp _staffHttp;

        public List<Staff>? Staff { get; set; }

        public StaffService(StaffHttp staffHttp)
        {
            _staffHttp = staffHttp;
        }

        public async Task<AuthResponse> Login(JWT.Requests.LoginRequest loginRequest)
        {
            AuthResponse authResponse = await _staffHttp.Login(loginRequest);

            return authResponse;
        }

        public async Task<AuthResponse> Lock(string emailAddress)
        {
            AuthResponse authResponse = await _staffHttp.Lock(emailAddress);

            return authResponse;
        }

        public async Task<List<Staff>> GetAllStaff(string token)
        {
            Staff = await _staffHttp.GetStaff(token);

            return Staff;
        }

        public async Task<Staff> GetStaff(int ID, string token)
        {
            Staff staff = await _staffHttp.GetStaff(ID, token);

            return staff;
        }

        public async Task<bool> CheckEmail(string email)
        {
            bool emailExists = await _staffHttp.CheckEmail(email);

            return emailExists;
        }

        public async Task<bool> ResetPassword(string email, string password)
        {
            bool passwordReset = await _staffHttp.ResetPassword(email, password);

            return passwordReset;
        }

        public async Task<Staff> CreateStaff(Staff staff, string token)
        {
            Staff newStaff = await _staffHttp.CreateStaff(staff, token);

            return newStaff;
        }

        public async Task<Staff> UpdateStaff(Staff staff, string token)
        {
            Staff updatedStaff = await _staffHttp.UpdateStaff(staff, token);

            return updatedStaff;
        }
    }
}
