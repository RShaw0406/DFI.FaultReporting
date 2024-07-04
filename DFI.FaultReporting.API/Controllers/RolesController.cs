using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.SQL.Repository.Admin;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private IRoleSQLRepository _roleSQLRepository;
        public ILogger<RolesController> _logger;

        public RolesController(IRoleSQLRepository roleSQLRepository, ILogger<RolesController> logger)
        {
            _roleSQLRepository = roleSQLRepository;
            _logger = logger;
        }

        public List<Role>? Roles { get; set; }

        // GET: api/Roles
        [HttpGet]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRole()
        {
            Roles = await _roleSQLRepository.GetRoles();
            return Roles;
        }

        // GET: api/Roles/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<Role>> GetRole(int ID)
        {
            Role role = await _roleSQLRepository.GetRole(ID);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        // POST: api/Roles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<Role>> PostRole(Role role)
        {
            role = await _roleSQLRepository.CreateRole(role);

            return CreatedAtAction("GetRole", new { role.ID }, role);
        }

        // PUT: api/Roles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<Role>> PutRole(Role role)
        {
            try
            {
                role = await _roleSQLRepository.UpdateRole(role);

                return role;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Roles = await _roleSQLRepository.GetRoles();

                if (!Roles.Any(cs => cs.ID == role.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/Roles/5
        [HttpDelete("{ID}")]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<int>> DeleteRole(int ID)
        {
            Role role = await _roleSQLRepository.GetRole(ID);

            if (role == null)
            {
                return NotFound();
            }

            await _roleSQLRepository.DeleteRole(ID);

            return ID;
        }
    }
}
