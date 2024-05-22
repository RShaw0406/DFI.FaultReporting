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

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimStatusController : ControllerBase
    {
        private IClaimStatusSQLRepository _claimStatusSQLRepository;
        public ILogger<ClaimStatusController> _logger;

        private readonly DFIFaultReportingDataContext _context;

        public ClaimStatusController(DFIFaultReportingDataContext context, IClaimStatusSQLRepository claimStatusSQLRepository, ILogger<ClaimStatusController> logger)
        {
            _context = context;
            _claimStatusSQLRepository = claimStatusSQLRepository;
            _logger = logger;
        }

        public List<ClaimStatus>? ClaimStatuses { get; set; }

        // GET: api/ClaimStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClaimStatus>>> GetClaimStatus()
        {
            ClaimStatuses = await _claimStatusSQLRepository.GetClaimStatuses();
            return ClaimStatuses;
        }

        // GET: api/ClaimStatus/5
        [HttpGet("{ID}")]
        public async Task<ActionResult<ClaimStatus>> GetClaimStatus(int ID)
        {
            ClaimStatus claimStatus = await _claimStatusSQLRepository.GetClaimStatus(ID);

            if (claimStatus == null)
            {
                return NotFound();
            }

            return claimStatus;
        }

        // POST: api/ClaimStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ClaimStatus>> PostClaimStatus(ClaimStatus claimStatus)
        {
            claimStatus = await _claimStatusSQLRepository.CreateClaimStatus(claimStatus);

            return CreatedAtAction("GetClaimStatus", new { claimStatus.ID }, claimStatus);
        }

        // PUT: api/ClaimStatus/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<ActionResult<ClaimStatus>> PutClaimStatus(ClaimStatus claimStatus)
        {
            try
            {
                claimStatus = await _claimStatusSQLRepository.UpdateClaimStatus(claimStatus);

                return claimStatus;
            }
            catch (DbUpdateConcurrencyException)
            {
                ClaimStatuses = await _claimStatusSQLRepository.GetClaimStatuses();

                if (!ClaimStatuses.Any(cs => cs.ID == claimStatus.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/ClaimStatus/5
        [HttpDelete("{ID}")]
        public async Task<ActionResult<int>> DeleteClaimStatus(int ID)
        {
            ClaimStatus claimStatus = await _claimStatusSQLRepository.GetClaimStatus(ID);

            if (claimStatus == null)
            {
                return NotFound();
            }

            await _claimStatusSQLRepository.DeleteClaimStatus(ID);

            return ID;
        }
    }
}
