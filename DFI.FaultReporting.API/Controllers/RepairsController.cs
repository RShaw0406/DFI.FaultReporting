using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.FaultReports;
using DFI.FaultReporting.SQL.Repository.FaultReports;
using NuGet.Protocol.Plugins;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairsController : ControllerBase
    {
        private IRepairSQLRepository _repairSQLRepository;
        public ILogger<RepairsController> _logger;

        public RepairsController(IRepairSQLRepository repairSQLRepository, ILogger<RepairsController> logger)
        {
            _repairSQLRepository = repairSQLRepository;
            _logger = logger;
        }

        public List<Repair>? Repairs { get; set; }

        // GET: api/Repairs
        [HttpGet]
        [Authorize (Roles = "Contractor, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<Repair>>> GetRepair()
        {
            Repairs = await _repairSQLRepository.GetRepairs();
            return Repairs;
        }

        // GET: api/Repairs/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "Contractor, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<Repair>> GetRepair(int ID)
        {
            Repair repair = await _repairSQLRepository.GetRepair(ID);

            if (repair == null)
            {
                return NotFound();
            }

            return repair;
        }

        // POST: api/Repairs
        [HttpPost]
        [Authorize(Roles = "StaffReadWrite")]
        public async Task<ActionResult<Repair>> PostRepair(Repair repair)
        {
            repair = await _repairSQLRepository.CreateRepair(repair);

            return CreatedAtAction("GetRepair", new { repair.ID }, repair);
        }

        // PUT: api/Repairs/5
        [HttpPut]
        [Authorize(Roles = "Contractor, StaffReadWrite")]
        public async Task<ActionResult<Repair>> PutRepair(Repair repair)
        {

            try
            {
                repair = await _repairSQLRepository.UpdateRepair(repair);

                return repair;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Repairs = await _repairSQLRepository.GetRepairs();

                if (!Repairs.Any(r => r.ID == repair.ID))
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
