using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.SQL.Repository.Roles;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffRolesController : ControllerBase
    {
        private IStaffRoleSQLRepository _staffRoleSQLRepository;
        public ILogger<StaffRolesController> _logger;

        public StaffRolesController(IStaffRoleSQLRepository staffRoleSQLRepository, ILogger<StaffRolesController> logger)
        {
            _staffRoleSQLRepository = staffRoleSQLRepository;
            _logger = logger;
        }

        public List<StaffRole>? StaffRoles { get; set; }

        // GET: api/StaffRoles
        [HttpGet]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<IEnumerable<StaffRole>>> GetStaffRole()
        {
            StaffRoles = await _staffRoleSQLRepository.GetStaffRoles();
            return StaffRoles;
        }

        // GET: api/StaffRoles/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<StaffRole>> GetStaffRole(int ID)
        {
            StaffRole staffRole = await _staffRoleSQLRepository.GetStaffRole(ID);

            if (staffRole == null)
            {
                return NotFound();
            }

            return staffRole;
        }

        // POST: api/StaffRoles
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<StaffRole>> PostStaffRole(StaffRole staffRole)
        {
            staffRole = await _staffRoleSQLRepository.CreateStaffRole(staffRole);

            return CreatedAtAction("GetStaffRole", new { staffRole.ID }, staffRole);
        }

        // PUT: api/StaffRoles/5
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<StaffRole>> PutStaffRole(StaffRole staffRole)
        {
            try
            {
                staffRole = await _staffRoleSQLRepository.UpdateStaffRole(staffRole);

                return staffRole;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                StaffRoles = await _staffRoleSQLRepository.GetStaffRoles();

                if (!StaffRoles.Any(cs => cs.ID == staffRole.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/StaffRoles/5
        [HttpDelete("{ID}")]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<int>> DeleteStaffRole(int ID)
        {
            StaffRole staffRole = await _staffRoleSQLRepository.GetStaffRole(ID);

            if (staffRole == null)
            {
                return NotFound();
            }

            await _staffRoleSQLRepository.DeleteStaffRole(ID);

            return ID;
        }
    }
}
