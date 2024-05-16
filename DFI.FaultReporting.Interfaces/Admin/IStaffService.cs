using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Admin
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
