using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Repairs
{
    public class FullScreenImageModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FullScreenImageModel> _logger;
        private readonly IRepairPhotoService _repairPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public FullScreenImageModel(ILogger<FullScreenImageModel> logger, IRepairPhotoService repairPhotoService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _repairPhotoService = repairPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        [BindProperty]
        public RepairPhoto RepairPhoto { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the selected repair photo is displayed on screen.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;

                    RepairPhoto = await _repairPhotoService.GetRepairPhoto(Convert.ToInt32(ID), jwtToken);

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
