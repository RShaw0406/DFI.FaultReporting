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
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRolesController : ControllerBase
    {
        private IUserRoleSQLRepository _userRoleSQLRepository;
        public ILogger<UserRolesController> _logger;

        public UserRolesController(IUserRoleSQLRepository userRoleSQLRepository, ILogger<UserRolesController> logger)
        {
            _userRoleSQLRepository = userRoleSQLRepository;
            _logger = logger;
        }

        public List<UserRole>? UserRoles { get; set; }

        // GET: api/UserRoles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRole()
        {
            UserRoles = await _userRoleSQLRepository.GetUserRoles();
            return UserRoles;
        }

        // GET: api/UserRoles/5
        [HttpGet("{ID}")]
        public async Task<ActionResult<UserRole>> GetUserRole(int ID)
        {
            UserRole userRole = await _userRoleSQLRepository.GetUserRole(ID);

            if (userRole == null)
            {
                return NotFound();
            }

            return userRole;
        }

        // POST: api/UserRoles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserRole>> PostUserRole(UserRole userRole)
        {
            userRole = await _userRoleSQLRepository.CreateUserRole(userRole);

            return CreatedAtAction("GetUserRole", new { userRole.ID }, userRole);
        }

        // PUT: api/UserRoles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<ActionResult<UserRole>> PutUserRole(UserRole userRole)
        {
            try
            {
                userRole = await _userRoleSQLRepository.UpdateUserRole(userRole);

                return userRole;
            }
            catch (DbUpdateConcurrencyException)
            {
                UserRoles = await _userRoleSQLRepository.GetUserRoles();

                if (!UserRoles.Any(cs => cs.ID == userRole.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/UserRoles/5
        [HttpDelete("{ID}")]
        public async Task<ActionResult<int>> DeleteUserRole(int ID)
        {
            UserRole userRole = await _userRoleSQLRepository.GetUserRole(ID);

            if (userRole == null)
            {
                return NotFound();
            }

            await _userRoleSQLRepository.DeleteUserRole(ID);

            return ID;
        }
    }
}
