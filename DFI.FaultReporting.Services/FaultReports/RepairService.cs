using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.FaultReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.FaultReports
{
    public class RepairService : IRepairService
    {
        private readonly RepairHttp _repairHttp;

        public List<Repair>? Repairs { get; set; }

        public RepairService(RepairHttp repairHttp)
        {
            _repairHttp = repairHttp;
        }

        public async Task<List<Repair>> GetRepairs(string token)
        {
            Repairs = await _repairHttp.GetRepairs(token);

            return Repairs;
        }

        public async Task<Repair> GetRepair(int ID, string token)
        {
            Repair repair = await _repairHttp.GetRepair(ID, token);

            return repair;
        }

        public async Task<Repair> CreateRepair(Repair repair, string token)
        {
            repair = await _repairHttp.CreateRepair(repair, token);

            return repair;
        }

        public async Task<Repair> UpdateRepair(Repair repair, string token)
        {
            repair = await _repairHttp.UpdateRepair(repair, token);

            return repair;
        }
    }
}
