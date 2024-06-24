using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class Step3Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step3Model> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IReportService _reportService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step3Model(ILogger<Step3Model> logger, IUserService userService, IReportService reportService, IFaultService faultService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
            _reportService = reportService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
        }
        #endregion Dependency Injection

        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
                    //Get the fault from "Fault" object stored in session.
                    Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

                    //Get the report from "Report" object stored in session.
                    Report? sessionReport = HttpContext.Session.GetFromSession<Report>("Report");

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
    }
}
