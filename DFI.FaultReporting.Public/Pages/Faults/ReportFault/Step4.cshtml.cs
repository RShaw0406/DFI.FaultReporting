using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Models.Admin;
using System.Security.Claims;
using SendGrid.Helpers.Mail;
using SendGrid;
using System.Net.Mail;

namespace DFI.FaultReporting.Public.Pages.Faults.ReportFault
{
    public class Step4Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step4Model> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IReportService _reportService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step4Model(ILogger<Step4Model> logger, IUserService userService, IReportService reportService, IFaultService faultService, IFaultTypeService faultTypeService,
            IReportPhotoService reportPhotoService, IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService, IFileValidationService fileValidationService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
            _faultTypeService = faultTypeService;
            _reportService = reportService;
            _reportPhotoService = reportPhotoService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
            _fileValidationService = fileValidationService;
        }
        #endregion Dependency Injection

        #region Properties
        //Declare CurrentUser property, this is needed when calling the _userService.
        public User CurrentUser { get; set; }

        //Declare Fault property, this is needed to store session Fault.
        public Fault Fault { get; set; }

        //Declare FaultType property, this is needed to store session Fault FaultType.
        public FaultType FaultType { get; set; }

        //Declare Report property, this is needed to store session Report.
        public Report Report { get; set; }

        //Declare ReportPhotos property, this is needed to store session ReportPhotos.
        public List<ReportPhoto> ReportPhotos { get; set; }
        #endregion Properties

        #region Page Load
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

                    //Get the fault from "Fault" object stored in session.
                    Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

                    //User has created Fault.
                    if (sessionFault != null) 
                    {
                        Fault = sessionFault;
                    }

                    FaultType = await _faultTypeService.GetFaultType(sessionFault.FaultTypeID, jwtToken);

                    //Get the report from "Report" object stored in session.
                    Report? sessionReport = HttpContext.Session.GetFromSession<Report>("Report");

                    //User has created Report.
                    if (sessionReport != null) 
                    {
                        Report = sessionReport;
                    }

                    //Get the report photos from "ReportPhotos" object stored in session.
                    List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

                    //User has uploaded images.
                    if (sessionReportPhotos != null && sessionReportPhotos.Count > 0)
                    {
                        ReportPhotos = sessionReportPhotos.ToList();
                    }

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

        #region Step4
        //Method Summary:
        //This method is executed when the submit button is clicked.
        //When executed the the session faul, report, and report photos list are inserted to the DB and the user is redirected to the SubmittedReport page.
        public async Task<IActionResult> OnPostSubmitReport()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentUser property by calling the GetUser method in the _userService.
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            //Get the fault from "Fault" object stored in session.
            Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Insert session fault to DB.
            Fault? insertedFault = await _faultService.CreateFault(sessionFault, jwtToken);

            //Get the report from "Report" object stored in session.
            Report? sessionReport = HttpContext.Session.GetFromSession<Report>("Report");

            sessionReport.FaultID = insertedFault.ID;

            //Insert session report to DB.
            Report? insertedReport = await _reportService.CreateReport(sessionReport, jwtToken);

            //Get the report photos from "ReportPhotos" object stored in session.
            List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

            if (sessionReportPhotos != null && sessionReportPhotos.Count > 0)
            {
                foreach (ReportPhoto reportPhoto in sessionReportPhotos)
                {
                    reportPhoto.ReportID = insertedReport.ID;

                    //Insert session reportPhoto to DB.
                    ReportPhoto? insertedReportPhoto = await _reportPhotoService.CreateReportPhoto(reportPhoto, jwtToken);
                }
            }

            //If the insertedReport is not null, send an email to the user.
            if (insertedReport != null)
            {
                //Send an email to the user.
                Response emailResponse = await SendSubmittedReportEmail(CurrentUser.Email);

                //If the email was sent successfully, redirect the user to the SubmittedReport page.
                return Redirect("/Faults/ReportFault/SubmittedReport");
            }
            //If the insertedReport is null, redirect the user to the Error page.
            else
            {
                //Redirect user to error page.
                return Redirect("/Error");
            }
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step3.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Faults/ReportFault/Step3");
        }

        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the user and returns the response from the _emailService.
        public async Task<Response> SendSubmittedReportEmail(string emailAddress)
        {
            //Set the subject of the email explaining that the user has deleted their account.
            string subject = "DFI Fault Reporting: Fault Report Submitted";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Set the htmlContent to a message explaining to the user that their account has been successfully deleted.
            string htmlContent = "<p>Hello,</p><p>Thank you for submitting a fault report.</p>" +
                "<p>DFI staff will now review your report and you will receive update emails as the status of the report progress.</p>" +
                "<br/>" +
                "<p><strong>Report details:</strong></p>" +
                "<p>"+ FaultType.FaultTypeDescription +"</p>" +
                "<p>"+ Fault.RoadNumber + ", " + Fault.RoadName + ", " + Fault.RoadTown + ", " + Fault.RoadCounty +"</p>" +
                "<p>"+ Report.AdditionalInfo + "</p>";

            //Set the attachment to null as it will not be used here.
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Step4
    }
}
