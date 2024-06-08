using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Token;
using DFI.FaultReporting.SQL.Repository.Interfaces.Roles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Token
{
    public class TokenService : ITokenService
    {
        public ISettingsService _settings { get; }
        private IUserRoleSQLRepository _userRoleSQLRepository;
        private IRoleSQLRepository _roleSQLRepository;

        public TokenService(ISettingsService settings, IUserRoleSQLRepository userRoleSQLRepository, IRoleSQLRepository roleSQLRepository)
        {
            _settings = settings;
            _userRoleSQLRepository = userRoleSQLRepository;
            _roleSQLRepository = roleSQLRepository;
        }

        public List<UserRole>? UserRoles { get; set; }

        public List<Role>? Roles { get; set; }

        public async Task<string> GenerateToken(User user)
        {
            UserRoles = await _userRoleSQLRepository.GetUserRoles();

            Roles = await _roleSQLRepository.GetRoles();

            List<UserRole> assignedUserRoles = UserRoles.Where(ur => ur.UserID == user.ID).ToList();

            List<Role> assignedRoles = new List<Role>();

            foreach (UserRole userRole in assignedUserRoles)
            {
                Role role = Roles.Where(r => r.ID == userRole.RoleID).FirstOrDefault();

                assignedRoles.Add(role);
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()));

            if (assignedRoles != null)
            {
                foreach (Role role in assignedRoles)
                {
                    claimsIdentity.AddClaim((new Claim(ClaimTypes.Role, role.RoleDescription)));
                }
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(await _settings.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTKEY));
            var issuer = await _settings.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTISSUER);
            var audience = await _settings.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTAUDIENCE);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = signingCredentials,
                Expires = DateTime.UtcNow.AddHours(1),
                Subject = claimsIdentity
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
