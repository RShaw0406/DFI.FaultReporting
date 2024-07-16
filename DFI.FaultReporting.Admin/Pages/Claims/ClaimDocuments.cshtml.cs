using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Admin.Pages.Claims
{
    public class ClaimDocumentsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ClaimDocumentsModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IClaimService _claimService;
        private readonly IClaimFileService _claimFileService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public ClaimDocumentsModel(ILogger<ClaimDocumentsModel> logger, IStaffService staffService, IClaimService claimService, IClaimFileService claimFileService,
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor, IClaimPhotoService claimPhotoService)
        {
            _logger = logger;
            _staffService = staffService;
            _claimService = claimService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _claimFileService = claimFileService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        public Claim Claim { get; set; }

        public List<ClaimFile> ClaimFiles { get; set; }

        public ClaimFile ClaimFile { get; set; }

        public bool ClaimHasDocuments { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the claim files from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {
                    //Get the current user ID and JWT token.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
                    System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    Claim = await _claimService.GetClaim(Convert.ToInt32(ID), jwtToken);

                    ClaimFiles = await _claimFileService.GetClaimFiles(jwtToken);

                    if (ClaimFiles != null)
                    {
                        ClaimHasDocuments = true;
                        ClaimFiles = ClaimFiles.Where(cf => cf.ClaimID == Claim.ID).ToList();
                    }

                    TempData["ClaimID"] = ID;
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

        #region Download File
        //Method Summary:
        //This method is called when the user clicks the download button.
        //It gets the file from the DB and downloads it for the user.
        public async Task<IActionResult> OnPostDownloadFile(int downloadFileValue)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Claim = await _claimService.GetClaim(Convert.ToInt32(TempData["ClaimID"]), jwtToken);

            ClaimFiles = await _claimFileService.GetClaimFiles(jwtToken);

            if (ClaimFiles != null)
            {
                ClaimHasDocuments = true;
                ClaimFiles = ClaimFiles.Where(cf => cf.ClaimID == Claim.ID).ToList();
            }

            ClaimFile = await _claimFileService.GetClaimFile(downloadFileValue, jwtToken);

            byte[] bytes = Convert.FromBase64String(ClaimFile.Data);

            string fileName = ClaimFile.Description;

            TempData.Keep();

            return File(bytes, "application/octet-stream", fileName);
        }
        #endregion Download File  
    }
}
