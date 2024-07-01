using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.SQL.Repository.Interfaces.Users;
using DFI.FaultReporting.SQL.Repository.Users;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using DFI.FaultReporting.JWT.Response;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.Wordprocessing;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.Models.Roles;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffsController : ControllerBase
    {
        private IStaffSQLRepository _staffSQLRepository;
        private IRoleSQLRepository _roleSQLRepository;
        private IStaffRoleSQLRepository _staffRoleSQLRepository;
        public ILogger<StaffsController> _logger;

        public StaffsController(IStaffSQLRepository staffSQLRepository, IRoleSQLRepository roleSQLRepository, IStaffRoleSQLRepository staffRoleSQLRepository, ILogger<StaffsController> logger)
        {
            _staffSQLRepository = staffSQLRepository;
            _roleSQLRepository = roleSQLRepository;
            _staffRoleSQLRepository = staffRoleSQLRepository;
            _logger = logger;
        }

        public List<Staff>? Staff { get; set; }

        // GET: api/Staffs
        [HttpGet]
        [Authorize(Roles = "StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<IEnumerable<Staff>>> GetStaff()
        {
            Staff = await _staffSQLRepository.GetStaff();
            return Staff;
        }

        // GET: api/Staffs/5
        [HttpGet("{ID}")]
        [Authorize(Roles = "StaffAdmin, StaffReadWrite, StaffRead")]
        public async Task<ActionResult<Staff>> GetStaff(int ID)
        {
            Staff staff = await _staffSQLRepository.GetStaff(ID);
            if (staff == null)
            {
                return NotFound();
            }

            return Ok(staff);
        }

        // POST: api/Staffs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "StaffAdmin")]
        public async Task<ActionResult<Staff>> PostStaff(Staff staff)
        {
            Staff = await _staffSQLRepository.GetStaff();

            Staff? currentStaff = Staff.Where(s => s.Email == staff.Email).FirstOrDefault();

            if (currentStaff != null)
            {
                return BadRequest(new AuthResponse { ReturnStatusCodeMessage = "Email address already used" });
            }

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: staff.Password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            staff.Password = passwordHash;
            staff.PasswordSalt = Convert.ToBase64String(salt);
            staff.AccountLocked = false;
            staff.AccountLockedEnd = null;
            staff.InputBy = "system admin";
            staff.InputOn = DateTime.Now;
            staff.Active = true;

            staff = await _staffSQLRepository.CreateStaff(staff);

            int roleID = 6;

            Role role = await _roleSQLRepository.GetRole(roleID);

            if (role == null)
            {
                return NotFound(new AuthResponse { ReturnStatusCodeMessage = "Role not found: StaffReadWrite" });
            }

            StaffRole staffReadRole = new StaffRole
            {
                RoleID = role.ID,
                StaffID = staff.ID,
                InputBy = "system admin",
                InputOn = DateTime.Now,
                Active = true
            };

            await _staffRoleSQLRepository.CreateStaffRole(staffReadRole);

            return CreatedAtAction("GetStaff", new { staff.ID }, staff);
        }

        // PUT: api/Staffs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        [Authorize(Roles = "StaffAdmin, StaffReadWrite")]
        public async Task<ActionResult<Staff>> PutStaff(Staff staff)
        {
            try
            {
                staff = await _staffSQLRepository.UpdateStaff(staff);

                return staff;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Staff = await _staffSQLRepository.GetStaff();

                if (!Staff.Any(cs => cs.ID == staff.ID))
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode((int)HttpStatusCode.BadRequest, ex.Message.ToString());
                }
            }
        }

        // DELETE: api/Staffs/5
        [HttpDelete("{ID}")]
        [Authorize(Roles = "StaffAdmin, StaffReadWrite")]
        public async Task<ActionResult<int>> DeleteStaff(int ID)
        {
            Staff staff = await _staffSQLRepository.GetStaff(ID);
            if (staff == null)
            {
                return NotFound();
            }

            await _staffSQLRepository.DeleteStaff(ID);

            return Ok(ID);
        }
    }
}
