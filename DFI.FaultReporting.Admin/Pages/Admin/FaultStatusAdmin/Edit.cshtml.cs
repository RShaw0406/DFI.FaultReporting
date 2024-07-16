using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.Security.Claims;
using DFI.FaultReporting.Services.Admin;

namespace DFI.FaultReporting.Admin.Pages.Admin.FaultStatusAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IFaultStatusService faultStatusService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
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
                    FaultStatus = await _faultStatusService.GetFaultStatus((int)ID, jwtToken);

                    //Store the fault status description in TempData.
                    TempData["Description"] = FaultStatus.FaultStatusDescription;
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

        #region Edit Fault Status
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the fault status in the DB.
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
                //Get all fault statuses from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<FaultStatus> faultStatuses = await _faultStatusService.GetFaultStatuses();

                //Fault status description has changed.
                if (FaultStatus.FaultStatusDescription != TempData["Description"].ToString())
                {
                    //Fault status already exists.
                    if (faultStatuses.Any(fs => fs.FaultStatusDescription == FaultStatus.FaultStatusDescription))
                    {
                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "Fault status already exists");
                        ModelState.AddModelError("FaultStatus.FaultStatusDescription", "Fault status already exists");

                        return Page();
                    }
                }

                //Update fault status in the DB.
                FaultStatus updatedFaultStatus = await _faultStatusService.UpdateFaultStatus(FaultStatus, jwtToken);

                //Fault status was updated.
                if (updatedFaultStatus != null)
                {
                    UpdateSuccess = true;

                    TempData["Description"] = updatedFaultStatus.FaultStatusDescription;
                    TempData.Keep();

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Fault status could not be updated");

                    TempData.Keep();

                    return Page();
                }
            }
        }
        #endregion Edit Fault Status

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
