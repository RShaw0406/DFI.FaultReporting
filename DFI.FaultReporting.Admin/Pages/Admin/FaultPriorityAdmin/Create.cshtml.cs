using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.FaultPriorityAdmin
{
    public class CreateModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<CreateModel> _logger;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public CreateModel(ILogger<CreateModel> logger, IFaultPriorityService faultPriorityService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
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
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    //Set the FaultPriority property to a new instance of FaultPriority.
                    FaultPriority = new FaultPriority();
                    FaultPriority.InputBy = CurrentStaff.Email;
                    FaultPriority.InputOn = DateTime.Now;
                    FaultPriority.Active = true;

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

        #region Create Fault Type
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new fault type and returns the user to the index page.
        public async Task<IActionResult> OnPostAsync()
        {
            //Model state is not valid.
            if (!ModelState.IsValid)
            {
                //Display each of the model errors.
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
            //ModelState is valid.
            else
            {
                await PopulateProperties();

                //Get all fault priorities from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<FaultPriority> faultPriorities = await _faultPriorityService.GetFaultPriorities();

                //Fault priority already exists.
                if (faultPriorities.Any(fp => fp.FaultPriorityDescription == FaultPriority.FaultPriorityDescription))
                {
                    ModelState.AddModelError(string.Empty, "Fault priority already exists");
                    ModelState.AddModelError("FaultPriority.FaultPriorityDescription", "Fault priority already exists");

                    return Page();
                }

                //Fault priority rating already exists.
                if (faultPriorities.Any(fp => fp.FaultPriorityRating == FaultPriority.FaultPriorityRating))
                {
                    ModelState.AddModelError(string.Empty, "Fault priority rating already exists");
                    ModelState.AddModelError("FaultPriority.FaultPriorityRating", "Fault priority rating already exists");

                    return Page();
                }

                //Create the new fault priority in the DB.
                FaultPriority insertedFaultPriority = await _faultPriorityService.CreateFaultPriority(FaultPriority, jwtToken);

                //If the fault priority was successfully created.
                if (insertedFaultPriority != null)
                {
                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the fault priority");

                    return Page();
                }
            }
        }
        #endregion Create Fault Type

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
