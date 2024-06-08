using DFI.FaultReporting.JWT.Requests;
using DFI.FaultReporting.JWT.Response;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DFI.FaultReporting.Public.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private IUserRoleService _userRoleService;
        private IRoleService _roleService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoginModel(IUserService userService, IUserRoleService userRoleService, IRoleService roleService, ILogger<LoginModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _userRoleService = userRoleService;
            _roleService = roleService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {             
                LoginRequest loginRequest = new LoginRequest();
                loginRequest.Email = Input.Email;
                loginRequest.Password = Input.Password;

                AuthResponse authResponse = await _userService.Login(loginRequest);

                if (authResponse != null)
                {
                    List<User> users = await _userService.GetUsers(authResponse.Token);

                    User user = users.Where(u => u.ID == authResponse.UserID).FirstOrDefault();

                    List<UserRole> UserRoles = await _userRoleService.GetUserRoles();

                    List<Role> Roles = await _roleService.GetRoles();

                    List<UserRole> assignedUserRoles = UserRoles.Where(ur => ur.UserID == user.ID).ToList();

                    List<Role> assignedRoles = new List<Role>();

                    foreach (UserRole userRole in assignedUserRoles)
                    {
                        Role role = Roles.Where(r => r.ID == userRole.RoleID).FirstOrDefault();

                        assignedRoles.Add(role);
                    }

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, authResponse.UserID.ToString()));
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, authResponse.UserName));
                    claimsIdentity.AddClaim(new Claim("Token", authResponse.Token));

                    if (assignedRoles != null)
                    {
                        foreach (Role role in assignedRoles)
                        {
                            claimsIdentity.AddClaim((new Claim(ClaimTypes.Role, role.RoleDescription)));
                        }
                    }



                    ClaimsPrincipal currentUser = new ClaimsPrincipal();
                    currentUser.AddIdentity(claimsIdentity);

                    HttpContext.User = currentUser;

                    var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, currentUser);


                    TempData["isAuth"] = true;
                    TempData["UserName"] = authResponse.UserName;
                    TempData.Keep();
                    ViewData["isAuth"] = true;
                    ViewData["UserName"] = authResponse.UserName;

                    _logger.LogInformation("User logged in.");
                    return Redirect("/Index");
                }
                else
                {
                    TempData["isAuth"] = null;
                    TempData["UserName"] = null;
                    TempData.Keep();
                    ViewData["isAuth"] = false;
                    ViewData["UserName"] = null;

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            return Page();
        }
    }
}
