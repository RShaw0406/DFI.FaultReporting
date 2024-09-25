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
        [Authorize(Roles = "User, StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<ClaimType>>> GetClaimType()
        {
            ClaimTypes = await _claimTypeSQLRepository.GetClaimTypes();
            return ClaimTypes;
        }

        // GET: api/ClaimTypes/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffAdmin, StaffReadWrite, StaffRead")]
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
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<ClaimType>> PostClaimType(ClaimType claimType)
        {
            claimType = await _claimTypeSQLRepository.CreateClaimType(claimType);

            return CreatedAtAction("GetClaimType", new { claimType.ID }, claimType);
        }

        // PUT: api/ClaimTypes
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<ClaimType>> PutClaimType(ClaimType claimType)
        {
            try
            {
                claimType = await _claimTypeSQLRepository.UpdateClaimType(claimType);

                return claimType;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ClaimTypes = await _claimTypeSQLRepository.GetClaimTypes();

                if (!ClaimTypes.Any(cs => cs.ID == claimType.ID))
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
