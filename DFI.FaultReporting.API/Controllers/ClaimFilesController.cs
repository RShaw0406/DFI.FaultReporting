using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Files;
using Microsoft.AspNetCore.Authorization;
using DFI.FaultReporting.SQL.Repository.Files;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimFilesController : ControllerBase
    {
        private IClaimFileSQLRepository _claimFileSQLRepository;
        public ILogger<ClaimFilesController> _logger;

        public ClaimFilesController(IClaimFileSQLRepository claimFileSQLRepository, ILogger<ClaimFilesController> logger)
        {
            _claimFileSQLRepository = claimFileSQLRepository;
            _logger = logger;
        }

        public List<ClaimFile>? ClaimFiles { get; set; }

        // GET: api/ClaimFiles
        [HttpGet]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<ClaimFile>>> GetClaimFile()
        {
            ClaimFiles = await _claimFileSQLRepository.GetClaimFiles();
            return ClaimFiles;
        }

        // GET: api/ClaimFiles/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<ClaimFile>> GetClaimFile(int ID)
        {
            ClaimFile claimFile = await _claimFileSQLRepository.GetClaimFile(ID);

            if (claimFile == null)
            {
                return NotFound();
            }

            return claimFile;
        }

        // POST: api/ClaimFiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ClaimFile>> PostClaimFile(ClaimFile claimFile)
        {
            await _claimFileSQLRepository.CreateClaimFile(claimFile);

            return CreatedAtAction("GetClaimFile", new { claimFile.ID }, claimFile);
        }

        // PUT: api/ClaimFiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ClaimFile>> PutClaimFile(ClaimFile claimFile)
        {
            try
            {
                claimFile = await _claimFileSQLRepository.UpdateClaimFile(claimFile);

                return claimFile;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                ClaimFiles = await _claimFileSQLRepository.GetClaimFiles();

                if (!ClaimFiles.Any(cf => cf.ID == claimFile.ID))
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
