using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class ReportImagesModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ReportImagesModel> _logger;
        private readonly IUserService _userService;
        private readonly IReportService _reportService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public ReportImagesModel(ILogger<ReportImagesModel> logger, IUserService userService, IReportService reportService, IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userService = userService;
            _reportService = reportService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Reports property, this is needed for returning the selected report from the DB.
        [BindProperty]
        public Report Report { get; set; }

        //Declare ReportPhotos property, this is needed to store returned report photos from the DB.
        [BindProperty]
        public List<ReportPhoto> ReportPhotos { get; set; }

        //Declare SelectedReportPhotos property, this is needed to store the photos for the selected report.
        [BindProperty]
        public List<ReportPhoto>? SelectedReportPhotos { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the images related to a reported fault are displayed on screen.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentUser property by calling the GetUser method in the _userService.
                    CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                    //Set the Report property by calling the GetReport method from the _reportService using the ID in TempData.
                    Report = await _reportService.GetReport(Convert.ToInt32(TempData["ReportID"]), jwtToken);

                    //Set the ReportPhotos property by calling the GetReportPhotos method from the _reportPhotoService using the ID in TempData.
                    ReportPhotos = await _reportPhotoService.GetReportPhotos(jwtToken);

                    //Create a new list of ReportPhoto objects.
                    SelectedReportPhotos = new List<ReportPhoto>();

                    //Loop through the ReportPhotos and add the photos to the SelectedReportPhotos list.
                    foreach (ReportPhoto reportPhoto in ReportPhotos)
                    {
                        //The ReportID in the ReportPhoto object matches the ID in the Report object.
                        if (reportPhoto.ReportID == Report.ID)
                        {
                            //Add the ReportPhoto to the SelectedReportPhotos list.
                            SelectedReportPhotos.Add(reportPhoto);
                        }
                    }

                    //Keep the TempData.
                    TempData.Keep();

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
            //The contexts current user does not exist.
            else
            {
                //Redirect user to no permission
                return Redirect("/NoPermission");
            }
        }
        #endregion Page Load

        #region View Fullscreen Photo
        //Method Summary:
        //This method is excuted when the "View Fullscreen Photo" button clicked
        //When executed the report photo ID is stored in TempData and the user is redirected to the FullScreenImage page.
        public async Task<IActionResult> OnGetFullScreenPhoto(int ID)
        {
            //Set the report ID in TempData.
            TempData["ReportPhotoID"] = ID;
            TempData.Keep();

            //Redirect user to the FullScreenImage page.
            return Redirect("/Faults/FullScreenImage");
        }
        #endregion View Fullscreen Photo

    }
}
