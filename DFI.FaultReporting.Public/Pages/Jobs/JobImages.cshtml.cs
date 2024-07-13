using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Users;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Jobs
{
    public class JobImagesModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<JobImagesModel> _logger;
        private readonly IUserService _userService;
        private readonly IRepairService _repairService;
        private readonly IRepairPhotoService _repairPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public JobImagesModel(ILogger<JobImagesModel> logger, IUserService userService, IRepairService repairService, IRepairPhotoService repairPhotoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userService = userService;
            _repairService = repairService;
            _repairPhotoService = repairPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        public Repair Repair { get; set; }

        [BindProperty]
        public List<RepairPhoto> RepairPhotos { get; set; }

        [BindProperty]
        public List<RepairPhoto>? SelectedRepairPhotos { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the images related to a repair are displayed on screen.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("Contractor"))
                {
                    //Clear session and tempdata to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties(ID);

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

        #region Data
        //Method Summary:
        //This method is excuted when the page loads.
        //When executed, it populates the page properties.
        public async Task PopulateProperties(int? ID)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            Repair = await _repairService.GetRepair(Convert.ToInt32(ID), jwtToken);

            RepairPhotos = await _repairPhotoService.GetRepairPhotos(jwtToken);

            SelectedRepairPhotos = RepairPhotos.Where(rp => rp.RepairID == Repair.ID).ToList();
        }
        #endregion Data
    }
}
