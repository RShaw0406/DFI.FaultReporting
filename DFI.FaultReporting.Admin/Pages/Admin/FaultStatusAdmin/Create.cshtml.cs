using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
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
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare FaultStatus property, this is needed when inputting new fault status to the DB.
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

                    //Set the FaultStatus property to a new instance of FaultStatus.
                    FaultStatus = new FaultStatus();
                    FaultStatus.InputBy = CurrentStaff.Email;
                    FaultStatus.InputOn = DateTime.Now;
                    FaultStatus.Active = true;

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

                //Get all fault statuses from the DB.
                List<FaultStatus> faultStatuses = await _faultStatusService.GetFaultStatuses();

                //Fault status already exists.
                if (faultStatuses.Any(fs => fs.FaultStatusDescription == FaultStatus.FaultStatusDescription))
                {
                    //Add model state error.
                    ModelState.AddModelError(string.Empty, "Fault status already exists");
                    ModelState.AddModelError("FaultStatus.FaultStatusDescription", "Fault status already exists");

                    //Return the page.
                    return Page();
                }

                //Create the new fault status in the DB.
                FaultStatus insertedFaultStatus = await _faultStatusService.CreateFaultStatus(FaultStatus, jwtToken);

                //If the fault status was successfully created.
                if (insertedFaultStatus != null)
                {
                    //Return to the index page.
                    return RedirectToPage("./Index");
                }
                //Fault status was not successfully created.
                else
                {
                    //Return an error message.
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the fault status");

                    //Return the page.
                    return Page();
                }
            }
        }
        #endregion Create Fault Status
    }
}
