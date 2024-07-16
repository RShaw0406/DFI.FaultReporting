using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.Security.Claims;
using DFI.FaultReporting.Services.Admin;

namespace DFI.FaultReporting.Admin.Pages.Admin.FaultStatusAdmin
{
    public class CreateModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<CreateModel> _logger;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public CreateModel(ILogger<CreateModel> logger, IFaultStatusService faultStatusService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _faultStatusService = faultStatusService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public FaultStatus FaultStatus { get; set; }
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

                    //Set the FaultStatus property to a new instance of FaultStatus.
                    FaultStatus = new FaultStatus();
                    FaultStatus.InputBy = CurrentStaff.Email;
                    FaultStatus.InputOn = DateTime.Now;
                    FaultStatus.Active = true;

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

        #region Create Fault Status
        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to create a new fault status and returns the user to the index page.
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

                //Get all fault statuses from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<FaultStatus> faultStatuses = await _faultStatusService.GetFaultStatuses();

                //Fault status already exists.
                if (faultStatuses.Any(fs => fs.FaultStatusDescription == FaultStatus.FaultStatusDescription))
                {
                    ModelState.AddModelError(string.Empty, "Fault status already exists");
                    ModelState.AddModelError("FaultStatus.FaultStatusDescription", "Fault status already exists");

                    return Page();
                }

                //Create the new fault status in the DB.
                FaultStatus insertedFaultStatus = await _faultStatusService.CreateFaultStatus(FaultStatus, jwtToken);

                //If the fault status was successfully created.
                if (insertedFaultStatus != null)
                {
                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the fault status");

                    return Page();
                }
            }
        }
        #endregion Create Fault Status

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
