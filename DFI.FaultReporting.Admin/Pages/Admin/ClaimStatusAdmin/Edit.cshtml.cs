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
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Users;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.ClaimStatusAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IClaimStatusService _claimStatusService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IClaimStatusService claimStatusService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _claimStatusService = claimStatusService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare ClaimStatus property, this is needed when inputting a new claim status.
        [BindProperty]
        public ClaimStatus ClaimStatus { get; set; }

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

                    //Get claim status from the DB.
                    ClaimStatus = await _claimStatusService.GetClaimStatus((int)ID, jwtToken);

                    //Store the claim status description in TempData.
                    TempData["Description"] = ClaimStatus.ClaimStatusDescription;

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

        #region Edit Claim Status
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the claim status in the DB.
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

                //Get all claim statuses from the DB.
                List<ClaimStatus> claimStatuses = await _claimStatusService.GetClaimStatuses(jwtToken);

                //Claim status description has changed.
                if (ClaimStatus.ClaimStatusDescription != TempData["Description"].ToString())
                {
                    //Claim status already exists.
                    if (claimStatuses.Any(cs => cs.ClaimStatusDescription == ClaimStatus.ClaimStatusDescription))
                    {
                        //Keep the TempData.
                        TempData.Keep();

                        //Add model state error.
                        ModelState.AddModelError(string.Empty, "Claim status already exists");
                        ModelState.AddModelError("ClaimStatus.ClaimStatusDescription", "Claim status already exists");

                        //Return the page.
                        return Page();
                    }
                }

                //Update claim status in the DB.
                ClaimStatus updatedClaimStatus = await _claimStatusService.UpdateClaimStatus(ClaimStatus, jwtToken);

                //Claim status was updated.
                if (updatedClaimStatus != null)
                {
                    //Set the UpdateSuccess property to true.
                    UpdateSuccess = true;

                    //Store the claim status description in TempData.
                    TempData["Description"] = updatedClaimStatus.ClaimStatusDescription;

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
                //Claim status could not be updated.
                else
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Claim status could not be updated");

                    //Keep the TempData.
                    TempData.Keep();

                    //Return the page.
                    return Page();
                }
            }
        }
        #endregion Edit Claim Status
    }
}
