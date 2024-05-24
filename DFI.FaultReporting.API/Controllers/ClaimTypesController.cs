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

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimTypesController : ControllerBase
    {
        private IClaimTypeSQLRepository _claimTypeSQLRepository;
        public ILogger<ClaimTypesController> _logger;

        public ClaimTypesController(IClaimTypeSQLRepository claimTypeSQLRepository, ILogger<ClaimTypesController> logger)
        {
            _claimTypeSQLRepository = claimTypeSQLRepository;
            _logger = logger;
        }

        public List<ClaimType>? ClaimTypes { get; set; }

        // GET: api/ClaimTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClaimType>>> GetClaimType()
        {
            ClaimTypes = await _claimTypeSQLRepository.GetClaimTypes();
            return ClaimTypes;
        }

        // GET: api/ClaimTypes/5
        [HttpGet("{ID}")]
        public async Task<ActionResult<ClaimType>> GetClaimType(int ID)
        {
            ClaimType claimType = await _claimTypeSQLRepository.GetClaimType(ID);

            if (claimType == null)
            {
                return NotFound();
            }

            return claimType;
        }

        // POST: api/ClaimTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ClaimType>> PostClaimType(ClaimType claimType)
        {
            claimType = await _claimTypeSQLRepository.CreateClaimType(claimType);

            return CreatedAtAction("GetClaimType", new { claimType.ID }, claimType);
        }

        // PUT: api/ClaimTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<ActionResult<ClaimType>> PutClaimType(ClaimType claimType)
        {
            try
            {
                claimType = await _claimTypeSQLRepository.UpdateClaimType(claimType);

                return claimType;
            }
            catch (DbUpdateConcurrencyException)
            {
                ClaimTypes = await _claimTypeSQLRepository.GetClaimTypes();

                if (!ClaimTypes.Any(cs => cs.ID == claimType.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/ClaimTypes/5
        [HttpDelete("{ID}")]
        public async Task<ActionResult<int>> DeleteClaimType(int ID)
        {
            ClaimType claimType = await _claimTypeSQLRepository.GetClaimType(ID);

            if (claimType == null)
            {
                return NotFound();
            }

            await _claimTypeSQLRepository.DeleteClaimType(ID);

            return ID;
        }
    }
}
