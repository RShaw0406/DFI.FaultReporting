using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.SQL.Repository.Contexts;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Roles;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Models.Roles;
using DFI.FaultReporting.Models.Users;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;
using System.Security.Claims;
using DFI.FaultReporting.Services.Settings;

namespace DFI.FaultReporting.Admin.Pages.Admin.ClaimStatusAdmin
{
    public class IndexModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<IndexModel> _logger;
        private readonly IClaimStatusService _claimStatusService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public IndexModel(ILogger<IndexModel> logger, IClaimStatusService claimStatusService, IStaffService staffService, IHttpContextAccessor httpContextAccessor, 
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

        //Declare ClaimStatuses property, this is needed when getting all claim statuses from the DB.
        [BindProperty]
        public List<ClaimStatus> ClaimStatuses { get; set; }

        //Declare PagedClaimStatuses property, this is needed for displaying claim statuses in the table.
        [BindProperty]
        public List<ClaimStatus> PagedClaimStatuses { get; set; }

        //Declare Pager property, this is needed for pagination.
        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the current user and all claim statuses from the DB.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role
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

                    //Set the ClaimStatuses property by calling the GetClaimStatuses method in the _claimStatusService.
                    ClaimStatuses = await _claimStatusService.GetClaimStatuses(jwtToken);

                    //Set the current page to 1.
                    Pager.CurrentPage = 1;

                    //Set the page size to the value from the settings.
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);

                    //Set the pager to the count of claim statuses.
                    Pager.Count = ClaimStatuses.Count;

                    //Set the PagedClaimStatuses property by calling the GetPaginatedClaimStatuses method in the _pagerService.
                    PagedClaimStatuses = await _pagerService.GetPaginatedClaimStatuses(ClaimStatuses, Pager.CurrentPage, Pager.PageSize);

                    //Return the page.
                    return Page();
                }
                //The contexts current user has not been authenticated.
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

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of claim statuses is displayed.
        public async void OnGetPaging()
        {
            //Set the pager count to the number of staff.
            Pager.Count = ClaimStatuses.Count;

            //Get the page of claimStatuses by calling the GetPaginatedClaimStatuses method from the _pagerService.
            PagedClaimStatuses = await _pagerService.GetPaginatedClaimStatuses(ClaimStatuses, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination
    }
}
