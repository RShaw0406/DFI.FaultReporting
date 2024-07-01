using DFI.FaultReporting.Common.Constants;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
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

namespace DFI.FaultReporting.Services.Tokens
{
    public class JWTTokenService : IJWTTokenService
    {
        public ISettingsService _settingsService;
        private IUserRoleSQLRepository _userRoleSQLRepository;
        private IStaffRoleSQLRepository _staffRoleSQLRepository;
        private IRoleSQLRepository _roleSQLRepository;

        public JWTTokenService(ISettingsService settingsService, IUserRoleSQLRepository userRoleSQLRepository, IStaffRoleSQLRepository staffRoleSQLRepository, IRoleSQLRepository roleSQLRepository)
        {
            _settingsService = settingsService;
            _userRoleSQLRepository = userRoleSQLRepository;
            _staffRoleSQLRepository = staffRoleSQLRepository;
            _roleSQLRepository = roleSQLRepository;
        }

        public List<UserRole>? UserRoles { get; set; }

        public List<StaffRole>? StaffRoles { get; set; }

        public List<Role>? Roles { get; set; }

        public async Task<SecurityToken> GenerateToken(User? user, Staff? staff)
        {
            UserRoles = await _userRoleSQLRepository.GetUserRoles();

            StaffRoles = await _staffRoleSQLRepository.GetStaffRoles();

            Roles = await _roleSQLRepository.GetRoles();

            if (user != null)
            {
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

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTKEY));
                string issuer = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTISSUER);
                string audience = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTAUDIENCE);
                DateTime expires = DateTime.Now.AddHours(1);
                SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = signingCredentials,
                    Expires = expires,
                    Subject = claimsIdentity
                };

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                return token;
            }

            if (staff != null)
            {
                List<StaffRole> assignedStaffRoles = StaffRoles.Where(sr => sr.StaffID == staff.ID).ToList();

                List<Role> assignedRoles = new List<Role>();

                foreach (StaffRole staffRole in assignedStaffRoles)
                {
                    Role role = Roles.Where(r => r.ID == staffRole.RoleID).FirstOrDefault();

                    assignedRoles.Add(role);
                }

                ClaimsIdentity claimsIdentity = new ClaimsIdentity();
                claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, staff.ID.ToString()));

                if (assignedRoles != null)
                {
                    foreach (Role role in assignedRoles)
                    {
                        claimsIdentity.AddClaim((new Claim(ClaimTypes.Role, role.RoleDescription)));
                    }
                }

                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes(await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTKEY));
                string issuer = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTISSUER);
                string audience = await _settingsService.GetSettingString(DFI.FaultReporting.Common.Constants.Settings.JWTAUDIENCE);
                DateTime expires = DateTime.Now.AddHours(1);
                SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = signingCredentials,
                    Expires = expires,
                    Subject = claimsIdentity
                };

                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
                return token;
            }

            return null;
        }
    }
}