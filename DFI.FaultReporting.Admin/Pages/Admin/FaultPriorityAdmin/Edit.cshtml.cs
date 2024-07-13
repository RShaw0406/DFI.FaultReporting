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
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public FaultPriority FaultPriority { get; set; }

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
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    //Get fault priority from the DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    FaultPriority = await _faultPriorityService.GetFaultPriority((int)ID, jwtToken);

                    //Store the fault priority description and rating in TempData.
                    TempData["Description"] = FaultPriority.FaultPriorityDescription;
                    TempData["Rating"] = FaultPriority.FaultPriorityRating;
                    TempData.Keep();

                    return Page();
                }
                else
                {
                    return Redirect("/NoPermission");
                }
            }
            else
            {
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
                //Display each of the model state errors.
                foreach (var item in ModelState)
                {
                    if (item.Value.Errors.Count > 0)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                return Page();
            }
            //Modelstate is valid.
            else
            {
                //Get all fault priorities from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<FaultPriority> faultPriorities = await _faultPriorityService.GetFaultPriorities();

                //Fault priority description has changed.
                if (FaultPriority.FaultPriorityDescription != TempData["Description"].ToString())
                {
                    //Fault priority already exists.
                    if (faultPriorities.Any(fp => fp.FaultPriorityDescription == FaultPriority.FaultPriorityDescription))
                    {
                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "Fault priority already exists");
                        ModelState.AddModelError("FaultPriority.FaultPriorityDescription", "Fault priority already exists");

                        return Page();
                    }
                }

                //Fault priority rating has changed.
                if (FaultPriority.FaultPriorityRating != TempData["Rating"].ToString())
                {
                    //Fault priority already exists.
                    if (faultPriorities.Any(fp => fp.FaultPriorityRating == FaultPriority.FaultPriorityRating))
                    {
                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "Fault priority rating already exists");
                        ModelState.AddModelError("FaultPriority.FaultPriorityRating", "Fault priority rating already exists");

                        return Page();
                    }
                }

                //Update fault priority in the DB.
                FaultPriority updatedFaultPriority = await _faultPriorityService.UpdateFaultPriority(FaultPriority, jwtToken);

                //Fault priority was updated.
                if (updatedFaultPriority != null)
                {
                    UpdateSuccess = true;

                    TempData["Description"] = updatedFaultPriority.FaultPriorityDescription;
                    TempData["Rating"] = updatedFaultPriority.FaultPriorityRating;

                    TempData.Keep();

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Fault priority could not be updated");

                    TempData.Keep();

                    return Page();
                }
            }
        }
        #endregion Edit Fault Type

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);
        }
        #endregion Data
    }
}
