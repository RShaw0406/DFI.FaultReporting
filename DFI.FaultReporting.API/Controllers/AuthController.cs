using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using DFI.FaultReporting.SQL.Repository.Interfaces.Users;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Security.Cryptography;

namespace DFI.FaultReporting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserSQLRepository _userSQLRepository;
        private IRoleSQLRepository _roleSQLRepository;
        private IUserRoleSQLRepository _userRoleSQLRepository;
        private IJWTTokenService _tokenService;
        public ILogger<AuthController> _logger;

        public AuthController(IUserSQLRepository userSQLRepository, IRoleSQLRepository roleSQLRepository, IUserRoleSQLRepository userRoleSQLRepository, ILogger<AuthController> logger, IJWTTokenService tokenService)
        {
            _userSQLRepository = userSQLRepository;
            _roleSQLRepository = roleSQLRepository;
            _userRoleSQLRepository = userRoleSQLRepository;
            _logger = logger;
            _tokenService = tokenService;
        }

        public List<User>? Users { get; set; }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            // Generate a 128-bit salt using a sequence of
            // cryptographically strong random bytes.
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: request.Password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            User requestUser = new User
            {
                Email = request.Email,
                EmailConfirmed = true,
                Password = passwordHash,
                PasswordSalt = salt,
                Prefix = request.Prefix,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DOB = request.DOB,
                AddressLine1 = request.AddressLine1,
                AddressLine2 = request.AddressLine2,
                AddressLine3 = request.AddressLine3,
                Postcode = request.Postcode,
                ContactNumber = request.ContactNumber,
                AccountLocked = false,
                AccountLockedEnd = null,
                IncorrectAttempts = 0,
                InputBy = request.Email,
                InputOn = DateTime.Now,
                Active = true
            };

            int roleID = 1;

            Role role = await _roleSQLRepository.GetRole(roleID);

            if (role == null)
            {
                return NotFound("Role not found: User");
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

            var token = await _tokenService.GenerateToken(requestUser);

            return Ok(new AuthResponse { Token = token.ToString() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]  DFI.FaultReporting.JWT.Requests.LoginRequest request)
        {
            Users = await _userSQLRepository.GetUsers();

            User? requestUser = Users.Where(u => u.Email == request.Email).FirstOrDefault();

            if (requestUser == null)
            {
                return Unauthorized("Invalid email");
            }

            byte[] salt = requestUser.PasswordSalt;

            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string passwordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: request.Password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            //bool verified = BCrypt.Net.BCrypt.Compare(requestUser.Password, passwordHash);

            if (passwordHash != requestUser.Password)
            {
                return Unauthorized("Invalid password");
            }

            // Generate token
            var token = await _tokenService.GenerateToken(requestUser);

            return Ok(new AuthResponse {  UserID = requestUser.ID, UserName = requestUser.Email, Token = token.ToString() });
        }
    }
}