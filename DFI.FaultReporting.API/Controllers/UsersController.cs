using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.SQL.Repository.Interfaces.Users;
using DFI.FaultReporting.SQL.Repository.Roles;
using System.Data;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserSQLRepository _userSQLRepository;
        public ILogger<UsersController> _logger;

        public UsersController(IUserSQLRepository userSQLRepository, ILogger<UsersController> logger)
        {
            _userSQLRepository = userSQLRepository;
            _logger = logger;
        }

        public List<User>? Users { get; set; }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            Users = await _userSQLRepository.GetUsers();
            return Users;
        }

        // GET: api/Users/5
        [HttpGet("{ID}")]
        public async Task<ActionResult<User>> GetUser(int ID)
        {
            User user = await _userSQLRepository.GetUser(ID);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user = await _userSQLRepository.CreateUser(user);

            return CreatedAtAction("GetUser", new { user.ID }, user);
        }

        // PUT: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<ActionResult<User>> PutUser(User user)
        {
            try
            {
                user = await _userSQLRepository.UpdateUser(user);

                return user;
            }
            catch (DbUpdateConcurrencyException)
            {
                Users = await _userSQLRepository.GetUsers();

                if (!Users.Any(cs => cs.ID == user.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{ID}")]
        public async Task<ActionResult<int>> DeleteUser(int ID)
        {
            User user = await _userSQLRepository.GetUser(ID);

            if (user == null)
            {
                return NotFound();
            }

            await _userSQLRepository.DeleteUser(ID);

            return ID;
        }
    }
}
