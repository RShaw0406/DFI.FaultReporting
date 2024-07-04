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
    public class FaultPrioritiesController : ControllerBase
    {
        private IFaultPrioritySQLRepository _faultPrioritySQLRepository;
        public ILogger<FaultPrioritiesController> _logger;

        public FaultPrioritiesController(IFaultPrioritySQLRepository faultPrioritySQLRepository, ILogger<FaultPrioritiesController> logger)
        {
            _faultPrioritySQLRepository = faultPrioritySQLRepository;
            _logger = logger;
        }

        public List<FaultPriority>? FaultPriorities { get; set; }

        // GET: api/FaultPriorities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FaultPriority>>> GetFaultPriority()
        {
            FaultPriorities = await _faultPrioritySQLRepository.GetFaultPriorities();
            return FaultPriorities;
        }

        // GET: api/FaultPriorities/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<FaultPriority>> GetFaultPriority(int ID)
        {
            FaultPriority faultPriority = await _faultPrioritySQLRepository.GetFaultPriority(ID);

            if (faultPriority == null)
            {
                return NotFound();
            }

            return faultPriority;
        }

        // POST: api/FaultPriorities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<FaultPriority>> PostFaultPriority(FaultPriority faultPriority)
        {
            faultPriority = await _faultPrioritySQLRepository.CreateFaultPriority(faultPriority);

            return CreatedAtAction("GetFaultPriority", new { faultPriority.ID }, faultPriority);
        }

        // PUT: api/FaultPriorities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<FaultPriority>> PutFaultPriority(FaultPriority faultPriority)
        {
            try
            {
                faultPriority = await _faultPrioritySQLRepository.UpdateFaultPriority(faultPriority);

                return faultPriority;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                FaultPriorities = await _faultPrioritySQLRepository.GetFaultPriorities();

                if (!FaultPriorities.Any(cs => cs.ID == faultPriority.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/FaultPriorities/5
        [HttpDelete("{ID}")]
        [Authorize]
        public async Task<ActionResult<int>> DeleteFaultPriority(int ID)
        {
            FaultPriority faultPriority = await _faultPrioritySQLRepository.GetFaultPriority(ID);

            if (faultPriority == null)
            {
                return NotFound();
            }

            await _faultPrioritySQLRepository.DeleteFaultPriority(ID);

            return ID;
        }
    }
}
