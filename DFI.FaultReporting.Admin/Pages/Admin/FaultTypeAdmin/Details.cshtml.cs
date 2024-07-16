using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Admin;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.FaultTypeAdmin
{
    public class DetailsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<DetailsModel> _logger;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public DetailsModel(ILogger<DetailsModel> logger, IFaultTypeService faultTypeService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _faultTypeService = faultTypeService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public FaultType FaultType { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the fault type details from the DB.
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

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Get fault type from the DB.
                    FaultType = await _faultTypeService.GetFaultType((int)ID, jwtToken);

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
    }
}
