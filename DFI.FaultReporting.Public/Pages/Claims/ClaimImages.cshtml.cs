using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Claims
{
    public class ClaimImagesModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ClaimImagesModel> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimPhotoService _claimPhotoService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public ClaimImagesModel(ILogger<ClaimImagesModel> logger, IUserService userService, IClaimService claimService,
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor, IClaimPhotoService claimPhotoService)
        {
            _logger = logger;
            _userService = userService;
            _claimService = claimService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _claimPhotoService = claimPhotoService;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        public Claim Claim { get; set; }

        public List<ClaimPhoto> ClaimPhotos { get; set; }

        public bool ClaimHasPhotos { get; set; }
        #endregion Properties


        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the claim photos from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    //Get the current user ID and JWT token.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                    System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                    Claim = await _claimService.GetClaim(Convert.ToInt32(ID), jwtToken);

                    ClaimPhotos = await _claimPhotoService.GetClaimPhotos(jwtToken);

                    if (ClaimPhotos != null)
                    {
                        ClaimHasPhotos = true;
                        ClaimPhotos = ClaimPhotos.Where(cp => cp.ClaimID == Claim.ID).ToList();
                    }


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
