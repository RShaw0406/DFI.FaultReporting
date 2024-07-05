using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using DFI.FaultReporting.SQL.Repository.Admin;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractorsController : ControllerBase
    {
        private IContractorSQLRepository _contractorSQLRepository;
        public ILogger<ContractorsController> _logger;

        public ContractorsController(IContractorSQLRepository contractorSQLRepository, ILogger<ContractorsController> logger)
        {
            _contractorSQLRepository = contractorSQLRepository;
            _logger = logger;
        }

        public List<Contractor>? Contractors { get; set; }

        // GET: api/Contractors
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Contractor>>> GetContractor()
        {
            Contractors = await _contractorSQLRepository.GetContractors();
            return Contractors;
        }

        // GET: api/Contractors/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<Contractor>> GetContractor(int ID)
        {
            var contractor = await _contractorSQLRepository.GetContractor(ID);

            if (contractor == null)
            {
                return NotFound();
            }

            return contractor;
        }

        // POST: api/Contractors
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<Contractor>> PostContractor(Contractor contractor)
        {
            contractor = await _contractorSQLRepository.CreateContractor(contractor);

            return CreatedAtAction("GetContractor", new { contractor.ID }, contractor);
        }


        // PUT: api/Contractors/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<Contractor>> PutContractor(Contractor contractor)
        {
            try
            {
                contractor = await _contractorSQLRepository.UpdateContractor(contractor);

                return contractor;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Contractors = await _contractorSQLRepository.GetContractors();

                if (!Contractors.Any(cs => cs.ID == contractor.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }
    }
}
