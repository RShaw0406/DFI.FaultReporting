using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Admin
{
    public class FaultTypeService : IFaultTypeService
    {
        private readonly FaultTypeHttp _faultTypeHttp;

        public List<FaultType>? FaultTypes { get; set; }

        public FaultTypeService(FaultTypeHttp faultTypeHttp)
        {
            _faultTypeHttp = faultTypeHttp;
        }

        public async Task<List<FaultType>> GetFaultTypes()
        {
            FaultTypes = await _faultTypeHttp.GetFaultTypes();

            return FaultTypes;
        }

        public async Task<FaultType> GetFaultType(int ID, string token)
        {
            FaultType faultType = await _faultTypeHttp.GetFaultType(ID, token);

            return faultType;
        }

        public async Task<FaultType> CreateFaultType(FaultType faultType)
        {
            faultType = await _faultTypeHttp.CreateFaultType(faultType);

            return faultType;
        }

        public async Task<FaultType> UpdateFaultType(FaultType faultType)
        {
            faultType = await _faultTypeHttp.UpdateFaultType(faultType);

            return faultType;
        }

        public async Task<int> DeleteFaultType(int ID)
        {
            await _faultTypeHttp.DeleteFaultType(ID);

            return ID;
        }
    }
}
