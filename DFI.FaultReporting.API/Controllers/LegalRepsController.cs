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
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.Claims;
using Microsoft.AspNetCore.Authorization;
using DFI.FaultReporting.Models.Files;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LegalRepsController : ControllerBase
    {
        private ILegalRepSQLRepository _legalRepSQLRepository;
        public ILogger<LegalRepsController> _logger;

        public LegalRepsController(ILegalRepSQLRepository legalRepSQLRepository, ILogger<LegalRepsController> logger)
        {
            _legalRepSQLRepository = legalRepSQLRepository;
            _logger = logger;
        }

        public List<LegalRep>? LegalReps { get; set; }

        // GET: api/LegalReps
        [HttpGet]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<LegalRep>>> GetLegalRep()
        {
            LegalReps = await _legalRepSQLRepository.GetLegalReps();
            return LegalReps;
        }

        // GET: api/LegalReps/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<LegalRep>> GetLegalRep(int ID)
        {
            LegalRep legalRep = await _legalRepSQLRepository.GetLegalRep(ID);

            if (legalRep == null)
            {
                return NotFound();
            }

            return legalRep;
        }


        // POST: api/LegalReps
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<LegalRep>> PostLegalRep(LegalRep legalRep)
        {
            await _legalRepSQLRepository.CreateLegalRep(legalRep);

            return CreatedAtAction("GetLegalRep", new { legalRep.ID }, legalRep);
        }

        // PUT: api/LegalReps/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<LegalRep>> PutLegalRep(LegalRep legalRep)
        {
            try
            {
                legalRep = await _legalRepSQLRepository.UpdateLegalRep(legalRep);

                return legalRep;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                LegalReps = await _legalRepSQLRepository.GetLegalReps();

                if (!LegalReps.Any(lr => lr.ID == legalRep.ID))
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
