using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Users;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.RepairStatusAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IRepairStatusService _repairStatusService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IRepairStatusService repairStatusService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _repairStatusService = repairStatusService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public RepairStatus RepairStatus { get; set; }

        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the repair status details from the DB.
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

                    //Get repair status from the DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    RepairStatus = await _repairStatusService.GetRepairStatus((int)ID, jwtToken);

                    //Store the repair status description in TempData.
                    TempData["Description"] = RepairStatus.RepairStatusDescription;
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

        #region Edit Repair Status
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the repair status in the DB.
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
                await PopulateProperties();

                //Get all repair statuses from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<RepairStatus> repairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

                //Repair status description has changed.
                if (RepairStatus.RepairStatusDescription != TempData["Description"].ToString())
                {
                    //Repair status already exists.
                    if (repairStatuses.Any(rs => rs.RepairStatusDescription == RepairStatus.RepairStatusDescription))
                    {
                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "Repair status already exists");
                        ModelState.AddModelError("RepairStatus.RepairStatusDescription", "Repair status already exists");

                        return Page();
                    }
                }

                //Update repair status in the DB.
                RepairStatus updatedRepairStatus = await _repairStatusService.UpdateRepairStatus(RepairStatus, jwtToken);

                //Repair status was updated.
                if (updatedRepairStatus != null)
                {
                    UpdateSuccess = true;

                    TempData["Description"] = updatedRepairStatus.RepairStatusDescription;
                    TempData.Keep();

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Repair status could not be updated");

                    TempData.Keep();

                    return Page();
                }
            }
        }
        #endregion Edit Repair Status

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
