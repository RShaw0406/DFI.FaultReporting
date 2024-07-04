using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using DFI.FaultReporting.SQL.Repository.Admin;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaultsController : ControllerBase
    {
        private IFaultSQLRepository _faultSQLRepository;
        public ILogger<FaultsController> _logger;

        public FaultsController(IFaultSQLRepository faultSQLRepository, ILogger<FaultsController> logger)
        {
            _faultSQLRepository = faultSQLRepository;
            _logger = logger;
        }

        public List<Fault>? Faults { get; set; }

        // GET: api/Faults
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fault>>> GetFaults()
        {
            Faults = await _faultSQLRepository.GetFaults();
            return Faults;
        }

        // GET: api/Faults/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<Fault>> GetFault(int ID)
        {
            Fault fault = await _faultSQLRepository.GetFault(ID);

            if (fault == null)
            {
                return NotFound();
            }

            return fault;
        }

        // POST: api/Faults
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Fault>> PostFault(Fault fault)
        {          
            fault = await _faultSQLRepository.CreateFault(fault);

            return CreatedAtAction("GetFault", new { fault.ID }, fault);
        }

        // PUT: api/Faults
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<Fault>> PutFault(Fault fault)
        {
            try
            {
                fault = await _faultSQLRepository.UpdateFault(fault);

                return fault;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Faults = await _faultSQLRepository.GetFaults();

                if (!Faults.Any(cs => cs.ID == fault.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/Faults/5
        [HttpDelete("{ID}")]
        [Authorize]
        public async Task<ActionResult<int>> DeleteFault(int ID)
        {
            Fault fault = await _faultSQLRepository.GetFault(ID);

            if (fault == null)
            {
                return NotFound();
            }

            await _faultSQLRepository.DeleteFault(ID);

            return ID;
        }
    }
}
