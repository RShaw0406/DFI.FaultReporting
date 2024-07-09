using Azure.Core;
using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.SQL.Repository.Interfaces.Admin;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.SQL.Repository.Interfaces.Users;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserSQLRepository _userSQLRepository;
        private IStaffSQLRepository _staffSQLRepository;
        private IRoleSQLRepository _roleSQLRepository;
        private IUserRoleSQLRepository _userRoleSQLRepository;
        private IJWTTokenService _tokenService;
        public ILogger<AuthController> _logger;
        public ISettingsService _settingsService;
        public IContractorSQLRepository _contractorSQLRepository;

        public AuthController(IUserSQLRepository userSQLRepository, IStaffSQLRepository staffSQLRepository, IRoleSQLRepository roleSQLRepository, 
            IUserRoleSQLRepository userRoleSQLRepository, ILogger<AuthController> logger, IJWTTokenService tokenService, 
            ISettingsService settingsService, IContractorSQLRepository contractorSQLRepository)
        {
            _userSQLRepository = userSQLRepository;
            _staffSQLRepository = staffSQLRepository;
            _roleSQLRepository = roleSQLRepository;
            _userRoleSQLRepository = userRoleSQLRepository;
            _logger = logger;
            _tokenService = tokenService;
            _settingsService = settingsService;
            _contractorSQLRepository = contractorSQLRepository;
        }

        public List<User>? Users { get; set; }

        public List<Staff>? Staff { get; set; }

        public List<Contractor>? Contractors { get; set; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            Users = await _userSQLRepository.GetUsers();

            User? currentUser = Users.Where(u => u.Email == request.Email && u.Email != null).FirstOrDefault();

            if (currentUser != null)
            {
                //return BadRequest("Email address already used");
                return BadRequest(new AuthResponse { ReturnStatusCodeMessage = "Email address already used" });
            }

            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: request.Password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            DateTime? DOB = null;

            Contractors = await _contractorSQLRepository.GetContractors(); 

            Contractor? contractor = Contractors.Where(c => c.Email == request.Email && c.Email != null).FirstOrDefault();

            if (contractor == null)
            {
                //Create a string DOB by combining all the input DOB values.
                string inputDOB = request.YearDOB.ToString() + "-" + request.MonthDOB.ToString() + "-" + request.DayDOB.ToString();

                //Create a DOB datetime.
                DOB = DateTime.Parse(inputDOB);
            }

            User requestUser = new User
            {
                Email = request.Email,
                Password = passwordHash,
                PasswordSalt = Convert.ToBase64String(salt),
                Prefix = request.Prefix,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DOB = DOB,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                AddressLine3 = request.AddressLine3,
                Postcode = request.Postcode,
                ContactNumber = request.ContactNumber,
                AccountLocked = false,
                AccountLockedEnd = null,
                InputBy = request.Email,
                InputOn = DateTime.Now,
                Active = true
            };

            int roleID = 0;

            if (contractor != null)
            {
                roleID = 3;
            }
            else
            {
                roleID = 1;
            }

            Role role = await _roleSQLRepository.GetRole(roleID);

            if (role == null)
            {
                //return NotFound("Role not found: User");
                return NotFound(new AuthResponse { ReturnStatusCodeMessage = "Role not found" });
            }

            User createdUser = await _userSQLRepository.CreateUser(requestUser);

            UserRole userRole = new UserRole
            {
                RoleID = role.ID,
                UserID = createdUser.ID,
                InputBy = "System",
                InputOn = DateTime.Now,
                Active = true
            };

            await _userRoleSQLRepository.CreateUserRole(userRole);

            SecurityToken token = await _tokenService.GenerateToken(createdUser, null);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            string tokenString = tokenHandler.WriteToken(token);

            return Ok(new AuthResponse { UserID = createdUser.ID, UserName = createdUser.Email, Token = tokenString });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]  DFI.FaultReporting.JWT.Requests.LoginRequest request)
        {
            Users = await _userSQLRepository.GetUsers();

            Staff = await _staffSQLRepository.GetStaff(); 

            User? requestUser = Users.Where(u => u.Email == request.Email && u.Email != null).FirstOrDefault();

            Staff? requestStaff = Staff.Where(s => s.Email == request.Email && s.Email != null).FirstOrDefault();

            if (requestUser == null && requestStaff == null)
            {
                //return Unauthorized("Invalid email");
                return Unauthorized(new AuthResponse { ReturnStatusCodeMessage = "Invalid email" });
            }

            if (requestUser != null)
            {
                if (requestUser.AccountLocked == true && requestUser.AccountLockedEnd > DateTime.Now)
                {
                    //return Unauthorized("Account locked");
                    return Unauthorized(new AuthResponse { ReturnStatusCodeMessage = "Account locked" });
                }

                byte[] salt = Convert.FromBase64String(requestUser.PasswordSalt);

                string requestPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: request.Password!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                if (requestPasswordHash != requestUser.Password)
                {
                    //return Unauthorized("Invalid password");
                    return Unauthorized(new AuthResponse { ReturnStatusCodeMessage = "Invalid password" });
                }

                if (requestUser.AccountLocked == true && requestUser.AccountLockedEnd < DateTime.Now)
                {
                    requestUser.AccountLocked = false;
                    requestUser.AccountLockedEnd = null;

                    await _userSQLRepository.UpdateUser(requestUser);
                }

                SecurityToken token = await _tokenService.GenerateToken(requestUser, null);

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                string tokenString = tokenHandler.WriteToken(token);

                return Ok(new AuthResponse { UserID = requestUser.ID, UserName = requestUser.Email, Token = tokenString });
            }

            if (requestStaff != null)
            {
                if (requestStaff.AccountLocked == true && requestStaff.AccountLockedEnd > DateTime.Now)
                {
                    //return Unauthorized("Account locked");
                    return Unauthorized(new AuthResponse { ReturnStatusCodeMessage = "Account locked" });
                }

                byte[] salt = Convert.FromBase64String(requestStaff.PasswordSalt);

                string requestPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: request.Password!,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                if (requestPasswordHash != requestStaff.Password)
                {
                    //return Unauthorized("Invalid password");
                    return Unauthorized(new AuthResponse { ReturnStatusCodeMessage = "Invalid password" });
                }

                if (requestStaff.AccountLocked == true && requestStaff.AccountLockedEnd < DateTime.Now)
                {
                    requestStaff.AccountLocked = false;
                    requestStaff.AccountLockedEnd = null;

                    await _staffSQLRepository.UpdateStaff(requestStaff);
                }

                SecurityToken token = await _tokenService.GenerateToken(null, requestStaff);

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

                string tokenString = tokenHandler.WriteToken(token);

                return Ok(new AuthResponse { UserID = requestStaff.ID, UserName = requestStaff.Email, Token = tokenString });
            }

            return NotFound(new AuthResponse { ReturnStatusCodeMessage = "Account not found" });
        }

        [HttpPut("lock")]
        public async Task<IActionResult> LockAccount([FromBody]string emailAddress)
        {
            Users = await _userSQLRepository.GetUsers();

            Staff = await _staffSQLRepository.GetStaff();

            User? lockedUser = Users.Where(u => u.Email == emailAddress && u.Email != null).FirstOrDefault();

            Staff? lockedStaff = Staff.Where(s => s.Email == emailAddress && s.Email != null).FirstOrDefault();

            if (lockedUser != null)
            {
                lockedUser.AccountLocked = true;
                lockedUser.AccountLockedEnd = DateTime.Now.AddMinutes(30);

                await _userSQLRepository.UpdateUser(lockedUser);

                return Ok(new AuthResponse { ReturnStatusCodeMessage = "Account locked" });
            }

            if (lockedStaff != null)
            {
                lockedStaff.AccountLocked = true;
                lockedStaff.AccountLockedEnd = DateTime.Now.AddMinutes(30);

                await _staffSQLRepository.UpdateStaff(lockedStaff);

                return Ok(new AuthResponse { ReturnStatusCodeMessage = "Account locked" });
            }

            return NotFound(new AuthResponse { ReturnStatusCodeMessage = "Account not found" });
        }
    }
}