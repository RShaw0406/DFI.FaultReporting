using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Claims
{
    public class FullScreenImageModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FullScreenImageModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IClaimService _claimService;
        private readonly IClaimPhotoService _claimPhotoService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public FullScreenImageModel(ILogger<FullScreenImageModel> logger, IStaffService staffService, IClaimService claimService,
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor, IClaimPhotoService claimPhotoService)
        {
            _logger = logger;
            _staffService = staffService;
            _claimService = claimService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _claimPhotoService = claimPhotoService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public ClaimPhoto ClaimPhoto { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the claim location from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {
                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentUser property by calling the GetUser method in the _userService.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    ClaimPhoto = await _claimPhotoService.GetClaimPhoto(Convert.ToInt32((int)ID), jwtToken);

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
