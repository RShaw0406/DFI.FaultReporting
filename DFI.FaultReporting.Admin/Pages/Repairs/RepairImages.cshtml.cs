using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Repairs
{
    public class RepairImagesModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<RepairImagesModel> _logger;
        private readonly IRepairService _repairService;
        private readonly IRepairPhotoService _repairPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public RepairImagesModel(ILogger<RepairImagesModel> logger, IRepairService repairService, IRepairPhotoService repairPhotoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _repairService = repairService;
            _repairPhotoService = repairPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
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
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
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
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            Repair = await _repairService.GetRepair(Convert.ToInt32(ID), jwtToken);

            RepairPhotos = await _repairPhotoService.GetRepairPhotos(jwtToken);

            SelectedRepairPhotos = RepairPhotos.Where(rp => rp.RepairID == Repair.ID).ToList();
        }
        #endregion Data
    }
}
