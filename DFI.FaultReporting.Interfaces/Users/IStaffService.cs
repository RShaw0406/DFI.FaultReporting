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
        Task<List<Staff>> GetAllStaff();

        Task<Staff> GetStaff(int ID);

        Task<Staff> CreateStaff(Staff staff);

        Task<Staff> UpdateStaff(Staff staff);

        Task<Staff> DeleteStaff(int ID);
    }
}
