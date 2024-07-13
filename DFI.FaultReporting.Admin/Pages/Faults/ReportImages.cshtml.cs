using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Faults
{
    public class ReportImagesModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ReportImagesModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IReportService _reportService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public ReportImagesModel(ILogger<ReportImagesModel> logger, IStaffService staffService, IReportService reportService, IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _staffService = staffService;
            _reportService = reportService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Report Report { get; set; }

        [BindProperty]
        public List<ReportPhoto> ReportPhotos { get; set; }

        [BindProperty]
        public List<ReportPhoto>? SelectedReportPhotos { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the images related to a reported fault are displayed on screen.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite") || HttpContext.User.IsInRole("StaffRead"))
                {
                    await PopulateProperties((int)ID);

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
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties(int ID)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Report = await _reportService.GetReport((int)ID, jwtToken);

            ReportPhotos = await _reportPhotoService.GetReportPhotos(jwtToken);

            //Create a new list of ReportPhoto objects.
            SelectedReportPhotos = new List<ReportPhoto>();
            foreach (ReportPhoto reportPhoto in ReportPhotos)
            {
                if (reportPhoto.ReportID == Report.ID)
                {
                    SelectedReportPhotos.Add(reportPhoto);
                }
            }
        }
        #endregion Data
    }
}
