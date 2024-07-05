using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Admin;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.RoleAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IRoleService _roleService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IRoleService roleService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
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

        //Declare UpdateSuccess property, this is needed for displaying the updated success message.
        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the claim status details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
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

                    //Get role from the DB.
                    Role = await _roleService.GetRole((int)ID, jwtToken);

                    //Store the fault type description in TempData.
                    TempData["Description"] = Role.RoleDescription;

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            else
            {
                //Redirect user to no permission.
                return Redirect("/NoPermission");
            }
        }
        #endregion Page Load

        #region Edit Role
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the role in the DB.
        public async Task<IActionResult> OnPostAsync()
        {
            //Modelstate is not valid.
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
            //Modelstate is valid.
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

                //Role description has changed.
                if (Role.RoleDescription != TempData["Description"].ToString())
                {
                    //Role already exists.
                    if (roles.Any(r => r.RoleDescription == Role.RoleDescription))
                    {
                        //Keep the TempData.
                        TempData.Keep();

                        //Add model state error.
                        ModelState.AddModelError(string.Empty, "Role already exists");
                        ModelState.AddModelError("Role.RoleDescription", "Role already exists");

                        //Return the page.
                        return Page();
                    }
                }

                //Update role in the DB.
                Role updatedRole = await _roleService.UpdateRole(Role, jwtToken);

                //Role was updated.
                if (updatedRole != null)
                {
                    //Set the UpdateSuccess property to true.
                    UpdateSuccess = true;

                    //Store the role description in TempData.
                    TempData["Description"] = updatedRole.RoleDescription;

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
                //Role could not be updated.
                else
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Role could not be updated");

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
            }      
        }
        #endregion Edit Role
    }
}
