using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class FullScreenImageModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<FullScreenImageModel> _logger;
        private readonly IUserService _userService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Inject dependencies in constructor.
        public FullScreenImageModel(ILogger<FullScreenImageModel> logger, IUserService userService, IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userService = userService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare ReportPhoto property, this is needed to store returned report photo from the DB.
        [BindProperty]
        public ReportPhoto ReportPhoto { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the selected report photo is displayed on screen.
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
                    ReportPhoto = await _reportPhotoService.GetReportPhoto(Convert.ToInt32(TempData["ReportPhotoID"]), jwtToken);

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
    }
}
