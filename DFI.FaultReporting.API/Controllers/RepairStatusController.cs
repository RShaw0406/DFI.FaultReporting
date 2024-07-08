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
using DFI.FaultReporting.SQL.Repository.Admin;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RepairStatusController : ControllerBase
    {
        private IRepairStatusSQLRepository _repairStatusSQLRepository;
        public ILogger<RepairStatusController> _logger;

        public RepairStatusController(IRepairStatusSQLRepository repairStatusSQLRepository, ILogger<RepairStatusController> logger)
        {
            _repairStatusSQLRepository = repairStatusSQLRepository;
            _logger = logger;
        }

        public List<RepairStatus>? RepairStatuses { get; set; }

        // GET: api/RepairStatus
        [HttpGet]
        [Authorize(Roles = "Contractor, StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<RepairStatus>>> GetRepairStatus()
        {
            RepairStatuses = await _repairStatusSQLRepository.GetRepairStatuses();
            return RepairStatuses;
        }

        // GET: api/RepairStatus/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "Contractor, StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<RepairStatus>> GetRepairStatus(int ID)
        {
            RepairStatus repairStatus = await _repairStatusSQLRepository.GetRepairStatus(ID);

            if (repairStatus == null)
            {
                return NotFound();
            }

            return repairStatus;
        }

        // POST: api/RepairStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<RepairStatus>> PostRepairStatus(RepairStatus repairStatus)
        {
            repairStatus = await _repairStatusSQLRepository.CreateRepairStatus(repairStatus);

            return CreatedAtAction("GetRepairStatus", new { repairStatus.ID }, repairStatus);
        }

        // PUT: api/RepairStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<RepairStatus>> PutRepairStatus(RepairStatus repairStatus)
        {
            try
            {
                repairStatus = await _repairStatusSQLRepository.UpdateRepairStatus(repairStatus);

                return repairStatus;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                RepairStatuses = await _repairStatusSQLRepository.GetRepairStatuses();

                if (!RepairStatuses.Any(cs => cs.ID == repairStatus.ID))
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
