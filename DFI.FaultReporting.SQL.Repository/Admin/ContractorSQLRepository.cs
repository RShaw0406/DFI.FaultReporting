using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Admin
{
    public class ContractorSQLRepository : IContractorSQLRepository
    {
        public DFIFaultReportingDataContext _context;

        public ContractorSQLRepository(DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public List<Contractor>? Contractors { get; set; }

        public async Task<List<Contractor>> GetContractors()
        {
            Contractors = await _context.Contractor.ToListAsync();
            return Contractors;
        }

        public async Task<Contractor> GetContractor(int ID)
        {
            Contractor contractor = await _context.Contractor.FindAsync(ID);
            return contractor;
        }

        public async Task<Contractor> CreateContractor(Contractor contractor)
        {
            _context.Contractor.Add(contractor);
            await _context.SaveChangesAsync();
            return contractor;
        }

        public async Task<Contractor> UpdateContractor(Contractor contractor)
        {
            _context.Entry(contractor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return contractor;
        }
    }
}
