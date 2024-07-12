using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        private IClaimSQLRepository _claimSQLRepository;
        public ILogger<ClaimsController> _logger;

        public ClaimsController(IClaimSQLRepository claimSQLRepository, ILogger<ClaimsController> logger)
        {
            _claimSQLRepository = claimSQLRepository;
            _logger = logger;
        }

        public List<Claim>? Claims { get; set; }

        // GET: api/Claims
        [HttpGet]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<Claim>>> GetClaim()
        {
            Claims = await _claimSQLRepository.GetClaims();
            return Claims;
        }

        // GET: api/Claims/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<Claim>> GetClaim(int ID)
        {
            Claim claim = await _claimSQLRepository.GetClaim(ID);

            if (claim == null)
            {
                return NotFound();
            }

            return claim;
        }

        // POST: api/Claims
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Claim>> PostClaim(Claim claim)
        {
            claim = await _claimSQLRepository.CreateClaim(claim);
            return CreatedAtAction("GetClaim", new { claim.ID }, claim);
        }

        // PUT: api/Claims/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "StaffReadWrite")]
        public async Task<ActionResult<Claim>> PutClaim(Claim claim)
        {
            try
            {
                claim = await _claimSQLRepository.UpdateClaim(claim);

                return claim;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Claims = await _claimSQLRepository.GetClaims();

                if (!Claims.Any(c => c.ID == claim.ID))
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
