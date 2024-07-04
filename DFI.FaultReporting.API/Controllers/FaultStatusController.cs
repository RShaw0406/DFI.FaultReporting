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
using DFI.FaultReporting.SQL.Repository.Admin;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaultStatusController : ControllerBase
    {
        private IFaultStatusSQLRepository _faultStatusSQLRepository;
        public ILogger<FaultStatusController> _logger;

        public FaultStatusController(IFaultStatusSQLRepository faultStatusSQLRepository, ILogger<FaultStatusController> logger)
        {
            _faultStatusSQLRepository = faultStatusSQLRepository;
            _logger = logger;
        }

        public List<FaultStatus>? FaultStatuses { get; set; }

        // GET: api/FaultStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FaultStatus>>> GetFaultStatus()
        {
            FaultStatuses = await _faultStatusSQLRepository.GetFaultStatuses();
            return FaultStatuses;
        }

        // GET: api/FaultStatus/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<FaultStatus>> GetFaultStatus(int ID)
        {
            FaultStatus faultStatus = await _faultStatusSQLRepository.GetFaultStatus(ID);

            if (faultStatus == null)
            {
                return NotFound();
            }

            return faultStatus;
        }

        // POST: api/FaultStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<FaultStatus>> PostFaultStatus(FaultStatus faultStatus)
        {
            faultStatus = await _faultStatusSQLRepository.CreateFaultStatus(faultStatus);

            return CreatedAtAction("GetFaultStatus", new { faultStatus.ID }, faultStatus);
        }

        // PUT: api/FaultStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<FaultStatus>> PutFaultStatus(FaultStatus faultStatus)
        {
            try
            {
                faultStatus = await _faultStatusSQLRepository.UpdateFaultStatus(faultStatus);

                return faultStatus;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                FaultStatuses = await _faultStatusSQLRepository.GetFaultStatuses();

                if (!FaultStatuses.Any(cs => cs.ID == faultStatus.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/FaultStatus/5
        [HttpDelete("{ID}")]
        [Authorize]
        public async Task<ActionResult<int>> DeleteFaultStatus(int ID)
        {
            FaultStatus faultStatus = await _faultStatusSQLRepository.GetFaultStatus(ID);

            if (faultStatus == null)
            {
                return NotFound();
            }

            await _faultStatusSQLRepository.DeleteFaultStatus(ID);

            return ID;
        }
    }
}
