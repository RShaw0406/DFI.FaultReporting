using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Admin;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.RoleAdmin
{
    public class CreateModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<CreateModel> _logger;
        private readonly IRoleService _roleService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public CreateModel(ILogger<CreateModel> logger, IRoleService roleService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _roleService = roleService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Role property, this is needed when getting role from the DB.
        [BindProperty]
        public Role Role { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and returns the page.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffAdmin"))
                {
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    //Set the Role property to a new instance of Role.
                    Role = new Role();
                    Role.InputBy = CurrentStaff.Email;
                    Role.InputOn = DateTime.Now;
                    Role.Active = true;

                    //Return the page.
                    return Page();
                }
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            //The contexts current user has not been authenticated.
            else
            {
                //Redirect user to no permission.
                return Redirect("/NoPermission");
            }
        }
        #endregion

        #region Create Role
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new role and returns the user to the index page.
        public async Task<IActionResult> OnPostAsync()
        {
            //Model state is not valid.
            if (!ModelState.IsValid)
            {
                //Loop through all model state items.
                foreach (var item in ModelState)
                {
                    //If the model state error has errors, add them to the model state.
                    if (item.Value.Errors.Count > 0)
                    {
                        //Loop through all errors and add them to the model state.
                        foreach (var error in item.Value.Errors)
                        {
                            //Add the error to the model state.
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                //Return the page.
                return Page();
            }
            //ModelState is valid.
            else
            {
                //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                string? jwtToken = jwtTokenClaim.Value;

                //Set the CurrentStaff property by calling the GetUser method in the _userService.
                CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                //Get all roles from the DB.
                List<Role> roles = await _roleService.GetRoles(jwtToken);

                //Role already exists.
                if (roles.Any(r => r.RoleDescription == Role.RoleDescription))
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Role already exists");
                    ModelState.AddModelError("Role.RoleDescription", "Role already exists");

                    //Return the page.
                    return Page();
                }

                //Create the new role in the DB.
                Role insertedRole = await _roleService.CreateRole(Role, jwtToken);

                //If the role was successfully created.
                if (insertedRole != null)
                {
                    //Return to the index page.
                    return RedirectToPage("./Index");
                }
                //FRole was not successfully created.
                else
                {
                    //Return an error message.
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the role");

                    //Return the page.
                    return Page();
                }
            }
        }
        #endregion Create Role
    }
}