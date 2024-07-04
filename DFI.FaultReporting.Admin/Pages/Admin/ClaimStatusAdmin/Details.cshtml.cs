using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Models.Users;
using System.Security.Claims;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace DFI.FaultReporting.Admin.Pages.Admin.ClaimStatusAdmin
{
    public class DetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<DetailsModel> _logger;
        private readonly IClaimStatusService _claimStatusService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public DetailsModel(ILogger<DetailsModel> logger, IClaimStatusService claimStatusService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
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
    }
}
