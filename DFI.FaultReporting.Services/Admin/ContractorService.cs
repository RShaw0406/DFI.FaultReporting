using DFI.FaultReporting.Http.Admin;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Admin
{
    public class ContractorService : IContractorService
    {
        private readonly ContractorHttp _contractorHttp;

        public List<Contractor>? Contractors { get; set; }

        public ContractorService(ContractorHttp contractorHttp)
        {
            _contractorHttp = contractorHttp;
        }

        public async Task<List<Contractor>> GetContractors(string token)
        {
            Contractors = await _contractorHttp.GetContractors(token);

            return Contractors;
        }

        public async Task<Contractor> GetContractor(int ID, string token)
        {
            Contractor contractor = await _contractorHttp.GetContractor(ID, token);

            return contractor;
        }

        public async Task<bool> CheckForContractor(string email)
        {
            bool exists = await _contractorHttp.CheckForContractor(email);

            return exists;
        }

        public async Task<Contractor> CreateContractor(Contractor contractor, string token)
        {
            contractor = await _contractorHttp.CreateContractor(contractor, token);

            return contractor;
        }

        public async Task<Contractor> UpdateContractor(Contractor contractor, string token)
        {
            contractor = await _contractorHttp.UpdateContractor(contractor, token);

            return contractor;
        }
    }
}
