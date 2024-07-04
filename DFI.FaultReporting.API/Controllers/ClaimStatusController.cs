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
using System.Security.Claims;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimStatusController : ControllerBase
    {
        private IClaimStatusSQLRepository _claimStatusSQLRepository;
        public ILogger<ClaimStatusController> _logger;

        public ClaimStatusController(IClaimStatusSQLRepository claimStatusSQLRepository, ILogger<ClaimStatusController> logger)
        {
            _claimStatusSQLRepository = claimStatusSQLRepository;
            _logger = logger;
        }

        public List<ClaimStatus>? ClaimStatuses { get; set; }

        // GET: api/ClaimStatus
        [HttpGet]
        [Authorize(Roles = "User, StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<ClaimStatus>>> GetClaimStatus()
        {
            ClaimStatuses = await _claimStatusSQLRepository.GetClaimStatuses();
            return ClaimStatuses;
        }

        // GET: api/ClaimStatus/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffAdmin, StaffReadWrite, StaffRead")]
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
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<ClaimStatus>> PostClaimStatus(ClaimStatus claimStatus)
        {
            claimStatus = await _claimStatusSQLRepository.CreateClaimStatus(claimStatus);

            return CreatedAtAction("GetClaimStatus", new { claimStatus.ID }, claimStatus);
        }

        // PUT: api/ClaimStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<ClaimStatus>> PutClaimStatus(ClaimStatus claimStatus)
        {
            try
            {
                claimStatus = await _claimStatusSQLRepository.UpdateClaimStatus(claimStatus);

                return claimStatus;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ClaimStatuses = await _claimStatusSQLRepository.GetClaimStatuses();

                if (!ClaimStatuses.Any(cs => cs.ID == claimStatus.ID))
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
