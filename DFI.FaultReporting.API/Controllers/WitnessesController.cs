using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Claims;
using Microsoft.AspNetCore.Authorization;
using DFI.FaultReporting.SQL.Repository.Claims;
using System.Net;
using System.Security.Claims;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WitnessesController : ControllerBase
    {
        private IWitnessSQLRepository _witnessSQLRepository;
        public ILogger<WitnessesController> _logger;

        public WitnessesController(IWitnessSQLRepository witnessSQLRepository, ILogger<WitnessesController> logger)
        {
            _witnessSQLRepository = witnessSQLRepository;
            _logger = logger;
        }

        public List<Witness>? Witnesses { get; set; }

        // GET: api/Witnesses
        [HttpGet]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<Witness>>> GetWitness()
        {
            Witnesses = await _witnessSQLRepository.GetWitnesses();
            return Witnesses;
        }

        // GET: api/Witnesses/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "User, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<Witness>> GetWitness(int ID)
        {
            Witness witness = await _witnessSQLRepository.GetWitness(ID);

            if (witness == null)
            {
                return NotFound();
            }

            return witness;
        }

        // POST: api/Witnesses
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Witness>> PostWitness(Witness witness)
        {
            await _witnessSQLRepository.CreateWitness(witness);

            return CreatedAtAction("GetWitness", new { witness.ID }, witness);
        }

        // PUT: api/Witnesses/5
        [HttpPut]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<Witness>> PutWitness(Witness witness)
        {
            try
            {
                witness = await _witnessSQLRepository.UpdateWitness(witness);

                return witness;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Witnesses = await _witnessSQLRepository.GetWitnesses();

                if (!Witnesses.Any(c => c.ID == witness.ID))
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
