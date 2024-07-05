using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.FaultPriorityAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IFaultPriorityService faultPriorityService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _faultPriorityService = faultPriorityService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare FaultPriority property, this is needed when getting the fault from the DB.
        [BindProperty]
        public FaultPriority FaultPriority { get; set; }

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

                    //Get fault priority from the DB.
                    FaultPriority = await _faultPriorityService.GetFaultPriority((int)ID, jwtToken);

                    //Store the fault priority description and rating in TempData.
                    TempData["Description"] = FaultPriority.FaultPriorityDescription;
                    TempData["Rating"] = FaultPriority.FaultPriorityRating;

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

        #region Edit Fault Type
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the fault type in the DB.
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

                //Get all fault priorities from the DB.
                List<FaultPriority> faultPriorities = await _faultPriorityService.GetFaultPriorities();

                //Fault priority description has changed.
                if (FaultPriority.FaultPriorityDescription != TempData["Description"].ToString())
                {
                    //Fault priority already exists.
                    if (faultPriorities.Any(fp => fp.FaultPriorityDescription == FaultPriority.FaultPriorityDescription))
                    {
                        //Keep the TempData.
                        TempData.Keep();

                        //Add model state error.
                        ModelState.AddModelError(string.Empty, "Fault priority already exists");
                        ModelState.AddModelError("FaultPriority.FaultPriorityDescription", "Fault priority already exists");

                        //Return the page.
                        return Page();
                    }
                }

                //Fault priority rating has changed.
                if (FaultPriority.FaultPriorityRating != TempData["Rating"].ToString())
                {
                    //Fault priority already exists.
                    if (faultPriorities.Any(fp => fp.FaultPriorityRating == FaultPriority.FaultPriorityRating))
                    {
                        //Keep the TempData.
                        TempData.Keep();

                        //Add model state error.
                        ModelState.AddModelError(string.Empty, "Fault priority rating already exists");
                        ModelState.AddModelError("FaultPriority.FaultPriorityRating", "Fault priority rating already exists");

                        //Return the page.
                        return Page();
                    }
                }

                //Update fault priority in the DB.
                FaultPriority updatedFaultPriority = await _faultPriorityService.UpdateFaultPriority(FaultPriority, jwtToken);

                //Fault priority was updated.
                if (updatedFaultPriority != null)
                {
                    //Set the UpdateSuccess property to true.
                    UpdateSuccess = true;

                    //Store the fault priority description and rating in TempData.
                    TempData["Description"] = updatedFaultPriority.FaultPriorityDescription;
                    TempData["Rating"] = updatedFaultPriority.FaultPriorityRating;

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
                //Fault priority could not be updated.
                else
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Fault priority could not be updated");

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
            }
        }
        #endregion Edit Fault Type
    }
}
