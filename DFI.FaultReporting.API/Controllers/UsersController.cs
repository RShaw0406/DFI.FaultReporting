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
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using DFI.FaultReporting.Models.Admin;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Cryptography;

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
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetUser()
        {
            Users = await _userSQLRepository.GetUsers();
            return Users;
        }

        // GET: api/Users/5
        [HttpGet("{ID}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int ID)
        {
            User user = await _userSQLRepository.GetUser(ID);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/Users/5
        [HttpGet("check/{email}")]
        public async Task<ActionResult<bool>> CheckEmail(string email)
        {
            Users = await _userSQLRepository.GetUsers();

            User user = Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return false;
            }

            return true;
        }

        [HttpPut("resetpassword/{email}/{password}")]
        public async Task<ActionResult<bool>> ResetPassword(string email, string password)
        {
            Users = await _userSQLRepository.GetUsers();

            User user = Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                return false;
            }

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            user.PasswordSalt = Convert.ToBase64String(salt);
            user.Password = passwordHash;

            User updatedUser = await _userSQLRepository.UpdateUser(user);

            if (updatedUser == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user = await _userSQLRepository.CreateUser(user);

            return CreatedAtAction("GetUser", new { user.ID }, user);
        }

        // PUT: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize]
        public async Task<ActionResult<User>> PutUser(User user)
        {
            try
            {
                user = await _userSQLRepository.UpdateUser(user);

                return user;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Users = await _userSQLRepository.GetUsers();

                if (!Users.Any(cs => cs.ID == user.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{ID}")]
        [Authorize]
        public async Task<ActionResult<int>> DeleteUser(int ID)
        {
            User user = await _userSQLRepository.GetUser(ID);

            if (user == null)
            {
                return NotFound();
            }

            await _userSQLRepository.DeleteUser(ID);

            return Ok(ID);
        }
    }
}
