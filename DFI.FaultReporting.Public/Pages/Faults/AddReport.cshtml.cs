using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Claims;
using DFI.FaultReporting.Services.Interfaces.Files;
using SendGrid.Helpers.Mail;
using SendGrid;
using Newtonsoft.Json.Linq;

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class AddReportModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<AddReportModel> _logger;
        private readonly IUserService _userService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IReportService _reportService;
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IFileValidationService _fileValidationService;

        //Inject dependencies in constructor.
        public AddReportModel(ILogger<AddReportModel> logger, IUserService userService, IFaultService faultService, IFaultTypeService faultTypeService,
            IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IReportService reportService, IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService, IFileValidationService fileValidationService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
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

        //Declare Fault property, this is needed for displaying fault info.
        public Fault? Fault { get; set; }

        //Declare Road property, this is needed for displaying Road info.
        public string? Road { get; set; }

        //Declare FaultPriority property, this is needed for displaying priority description.
        public FaultPriority FaultPriority { get; set; }

        //Declare FaultStatus property, this is needed for displaying status description.
        public FaultStatus FaultStatus { get; set; }

        //Declare FaultType property, this is needed for displaying type description.
        public FaultType FaultType { get; set; }

        //Declare Report property, this is needed for inserting a new report
        public Report Report { get; set; }

        //Declare ReportPhotos property, this is needed for inserting new report photos.
        public List<ReportPhoto> ReportPhotos { get; set; }

        //Declare ReportDetailsInputModel property, this is needed when adding a report.
        [BindProperty]
        public ReportDetailsInputModel ReportDetailsInput { get; set; }

        //Declare ReportDetailsInputModel class, this is needed when adding a report.
        public class ReportDetailsInputModel
        {
            [DisplayName("Additional information")]
            [Required(ErrorMessage = "You must enter additional info")]
            [StringLength(1000, ErrorMessage = "Additional info must not be more than 1000 characters")]
            public string? AdditionalInfo { get; set; }
        }

        //Declare SelectedFile property, this is needed for storing individual photo each time one is uploaded.
        [Display(Name = "Fault photo")]
        [DataType(DataType.Upload)]
        public IFormFile? SelectedFile { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the details of the fault selected by the user are populated and displayed on screen.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true)
                {
                    //Retreive the ID in the url query string.
                    string ID = HttpContext.Request.Query["ID"].ToString();

                    //Convert ID to int.
                    int faultID = Convert.ToInt32(ID);

                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentUser property by calling the GetUser method in the _userService.
                    CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                    //Populate the Fault property by calling the GetFault method from the _faultService.
                    Fault = await _faultService.GetFault(faultID, jwtToken);

                    string faultRoad = Fault.RoadNumber + ", " + Fault.RoadName + ", " + Fault.RoadTown + ", " + Fault.RoadCounty;

                    Road = faultRoad.Replace(", undefined", "");
                    Road = Road.Replace("undefined, ", "");

                    //Populate the FaultType property by calling the GetFaultType method from the _faultTypeService.
                    FaultType = await _faultTypeService.GetFaultType(Fault.FaultTypeID, jwtToken);

                    //Populate the FaultStatus property by calling the GetFaultStatus method from the _faultStatusService.
                    FaultStatus = await _faultStatusService.GetFaultStatus(Fault.FaultStatusID, jwtToken);

                    //Populate the FaultPriority property by calling the GetFaultPriority method from the _faultPriorityService.
                    FaultPriority = await _faultPriorityService.GetFaultPriority(Fault.FaultPriorityID, jwtToken);

                    //Set the FaultType in session, needed for displaying details of fault user selected on map.
                    HttpContext.Session.SetInSession("FaultType", FaultType);

                    //Set the FaultStatus in session, needed for displaying details of fault user selected on map.
                    HttpContext.Session.SetInSession("FaultStatus", FaultStatus);

                    //Set the FaultPriority in session, needed for displaying details of fault user selected on map.
                    HttpContext.Session.SetInSession("FaultPriority", FaultPriority);

                    //Set the Fault in session, needed for displaying details of fault user selected on map.
                    HttpContext.Session.SetInSession("Fault", Fault);

                    //Get the report photos from "ReportPhotos" object stored in session.
                    List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

                    //User has previously uploaded images.
                    if (sessionReportPhotos != null && sessionReportPhotos.Count > 0)
                    {
                        ReportPhotos = sessionReportPhotos.ToList();
                    }

                    //Return the Page.
                    return Page();
                }
            }

            //Return the Page.
            return Page();
        }
        #endregion Page Load

        #region AddReport
        //Method Summary:
        //This method is executed when the upload button is clicked.
        //When excuted the selected file is validated and if valid is added to the ReportPhotos list and displayed on screen for the user.
        public async Task<IActionResult> OnPostUpload()
        {
            //Set the ReportDetailsInput in session, needed for maintaining additional details value after post.
            HttpContext.Session.SetInSession("ReportDetailsInput", ReportDetailsInput);

            //Get the fault from "Fault" object stored in session.
            Fault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Get the status from "FaultStatus" object stored in session.
            FaultStatus = HttpContext.Session.GetFromSession<FaultStatus>("FaultStatus");

            //Get the priority from "FaultPriority" object stored in session.
            FaultPriority = HttpContext.Session.GetFromSession<FaultPriority>("FaultPriority");

            //Get the type from "FaultType" object stored in session.
            FaultType = HttpContext.Session.GetFromSession<FaultType>("FaultType");

            //Get the additional details from "ReportDetailsInput" object stored in session.
            ReportDetailsInput = HttpContext.Session.GetFromSession<ReportDetailsInputModel>("ReportDetailsInput");

            //Get the report photos from "ReportPhotos" object stored in session.
            List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

            //User has previously uploaded images.
            if (sessionReportPhotos != null && sessionReportPhotos.Count > 0)
            {
                ReportPhotos = sessionReportPhotos.ToList();
            }
            //User has not uploaded any images.
            else
            {
                ReportPhotos = new List<ReportPhoto>();
            }

            //User has uploaded less than 5 photos.
            if (ReportPhotos.Count < 5)
            {
                //File has been selected in upload control.
                if (SelectedFile != null)
                {
                    //Initialise a new ValidationContext to be used to validate the SelectedFile model only.
                    ValidationContext validationContext = new ValidationContext(SelectedFile);

                    //Create a collection to store the returned SelectedFile model validation results.
                    ICollection<ValidationResult> validationResults = new List<ValidationResult>();

                    //Carry out validation check on the SelectedFile model.
                    bool isSelectedFileValid = Validator.TryValidateObject(SelectedFile, validationContext, validationResults, true);

                    //SelectedFile is valid.
                    if (isSelectedFileValid == true)
                    {
                        //File size is higher than 0 bytes.
                        if (SelectedFile.Length > 0)
                        {
                            //Validate the selected file.
                            bool fileIsValid = await _fileValidationService.ValidateDocument(SelectedFile);

                            //File is not valid e.g. unsupported file type, MIME type matches extension etc.
                            if (fileIsValid == false)
                            {
                                ModelState.AddModelError("SelectedFile", "Selected file must be in .jpg, .jpeg, or .png format");
                                return Page();
                            }

                            //File size is larger than 5MB.
                            if (SelectedFile.Length > 5242880)
                            {
                                ModelState.AddModelError("SelectedFile", "Selected file size must be 5MB or less");
                                return Page();
                            }

                            //Get FileName.
                            string fileName = Path.GetFileName(SelectedFile.FileName);

                            //Get file Extension.
                            string fileExtension = Path.GetExtension(fileName);

                            //File extensioln is not valid.
                            if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                            {
                                ModelState.AddModelError("SelectedFile", "Selected file must be in .jpg, .jpeg, or .png format");
                                return Page();
                            }

                            //Concatenating FileName + FileExtension.
                            var newFileName = String.Concat(Convert.ToString(Guid.NewGuid()), fileExtension);

                            //Get the bytes from the selected file.
                            byte[] fileBytes;
                            using (var target = new MemoryStream())
                            {
                                SelectedFile.CopyTo(target);
                                fileBytes = target.ToArray();
                            }

                            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                            string? jwtToken = jwtTokenClaim.Value;

                            //Set the CurrentUser property by calling the GetUser method in the _userService.
                            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                            //Create new ReportPhoto.
                            ReportPhoto? reportPhoto = new ReportPhoto();
                            reportPhoto.ReportID = 0;
                            reportPhoto.Description = fileName;
                            reportPhoto.Type = fileExtension;
                            reportPhoto.Data = Convert.ToBase64String(fileBytes);
                            reportPhoto.InputBy = CurrentUser.Email;
                            reportPhoto.InputOn = DateTime.Now;
                            reportPhoto.Active = true;

                            //Add the new photo to the list of photos.
                            ReportPhotos.Add(reportPhoto);

                            //Set the upload report in session list.
                            HttpContext.Session.SetInSession("ReportPhotos", ReportPhotos);

                            return Page();

                        }
                        //File size is 0 bytes.
                        else
                        {
                            //Add an error to the ModelState to inform the user of en validation errors.
                            ModelState.AddModelError("SelectedFile", "The selected file does not contain any data");

                            //Return the page.
                            return Page();
                        }
                    }
                    //SelectedFile is not valid.
                    else
                    {
                        //Loop over each validationResult in the returned validationResults
                        foreach (ValidationResult validationResult in validationResults)
                        {
                            //Add an error to the ModelState to inform the user of en validation errors.
                            ModelState.AddModelError("SelectedFile", validationResult.ErrorMessage);
                        }

                        //Return the Page.
                        return Page();
                    }
                }
                //No file has been selected in upload control.
                else
                {
                    //Add an error to the ModelState to inform the user of en validation errors.
                    ModelState.AddModelError("SelectedFile", "You have not selected any file to upload");

                    //Return the page.
                    return Page();
                }
            }
            //User has uploaded 5 photos.
            else
            {
                //Add an error to the ModelState to inform the user of en validation errors.
                ModelState.AddModelError("SelectedFile", "You can only upload a maximum of 5 photos");

                //Return the page.
                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the trash can button is clicked beneath any of the onscreen photos.
        //When executed the selected photo is removed from the ReportPhotos list.
        public async Task<IActionResult> OnPostRemovePhoto(string removePhotoValue)
        {
            //Set the ReportDetailsInput in session, needed for maintaining additional details value after post.
            HttpContext.Session.SetInSession("ReportDetailsInput", ReportDetailsInput);

            //Get the fault from "Fault" object stored in session.
            Fault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Get the status from "FaultStatus" object stored in session.
            FaultStatus = HttpContext.Session.GetFromSession<FaultStatus>("FaultStatus");

            //Get the priority from "FaultPriority" object stored in session.
            FaultPriority = HttpContext.Session.GetFromSession<FaultPriority>("FaultPriority");

            //Get the type from "FaultType" object stored in session.
            FaultType = HttpContext.Session.GetFromSession<FaultType>("FaultType");

            //Get the additional details from "ReportDetailsInput" object stored in session.
            ReportDetailsInput = HttpContext.Session.GetFromSession<ReportDetailsInputModel>("ReportDetailsInput");

            //Get the report photos from "ReportPhotos" object stored in session.
            List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

            //User has previously uploaded images.
            if (sessionReportPhotos != null && sessionReportPhotos.Count > 0)
            {
                ReportPhotos = sessionReportPhotos.ToList();
            }
            //User has not uploaded any images.
            else
            {
                ReportPhotos = new List<ReportPhoto>();
            }

            //Find the report photo to remove using the passed decription value.
            ReportPhoto? reportPhotoToRemove = ReportPhotos.Where(rp => rp.Description == removePhotoValue).FirstOrDefault();

            //The report photo to remove has been found.
            if (reportPhotoToRemove != null)
            {
                //Remove the photo.
                ReportPhotos.Remove(reportPhotoToRemove);
            }

            //Set the upload report in session list.
            HttpContext.Session.SetInSession("ReportPhotos", ReportPhotos);

            //Return the Page.
            return Page();

        }

        //Method Summary:
        //This method is executed when the submit button is clicked.
        //When executed the the session faul, report, and report photos list are inserted to the DB and the user is redirected to the SubmittedReport page.
        public async Task<IActionResult> OnPostSubmitReport()
        {
            //Set the ReportDetailsInput in session, needed for maintaining additional details value after post.
            HttpContext.Session.SetInSession("ReportDetailsInput", ReportDetailsInput);

            //Get the fault from "Fault" object stored in session.
            Fault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Get the status from "FaultStatus" object stored in session.
            FaultStatus = HttpContext.Session.GetFromSession<FaultStatus>("FaultStatus");

            //Get the priority from "FaultPriority" object stored in session.
            FaultPriority = HttpContext.Session.GetFromSession<FaultPriority>("FaultPriority");

            //Get the type from "FaultType" object stored in session.
            FaultType = HttpContext.Session.GetFromSession<FaultType>("FaultType");

            //Get the additional details from "ReportDetailsInput" object stored in session.
            ReportDetailsInput = HttpContext.Session.GetFromSession<ReportDetailsInputModel>("ReportDetailsInput");

            //Get the report photos from "ReportPhotos" object stored in session.
            List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

            if (ModelState.IsValid)
            {
                //User has previously uploaded images.
                if (sessionReportPhotos != null && sessionReportPhotos.Count > 0)
                {
                    ReportPhotos = sessionReportPhotos.ToList();
                }
                //User has not uploaded any images.
                else
                {
                    ReportPhotos = new List<ReportPhoto>();
                }

                //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                string? jwtToken = jwtTokenClaim.Value;

                //Set the CurrentUser property by calling the GetUser method in the _userService.
                CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

                //Create a new report object and populate.
                Report newReport = new Report();
                newReport.FaultID = Fault.ID;
                newReport.AdditionalInfo = ReportDetailsInput.AdditionalInfo;
                newReport.UserID = CurrentUser.ID;
                newReport.InputBy = CurrentUser.Email;
                newReport.InputOn = DateTime.Now;
                newReport.Active = true;

                //Insert session report to DB.
                Report? insertedReport = await _reportService.CreateReport(newReport, jwtToken);

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
                    Response emailResponse = await SendSubmittedReportEmail(CurrentUser.Email, insertedReport);

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

            //Return the Page.
            return Page();
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Faults.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Faults/Faults");
        }

        //Method Summary:
        //This method is executed when the "Submit" button is clicked.
        //When executed this method attempts to send an email to the user and returns the response from the _emailService.
        public async Task<Response> SendSubmittedReportEmail(string emailAddress, Report insertedReport)
        {
            //Set the subject of the email explaining that the user has deleted their account.
            string subject = "DFI Fault Reporting: Fault Report Submitted";

            //Declare a new EmailAddress object and assign the users email address as the value.
            EmailAddress to = new EmailAddress(emailAddress);

            //Set textContent to empty string as it will not be used here.
            string textContent = string.Empty;

            //Get the fault from "Fault" object stored in session.
            Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Get the FaultType from the sessionFault.
            FaultType = await _faultTypeService.GetFaultType(sessionFault.FaultTypeID, jwtToken);

            //Set the htmlContent to a message explaining to the user that their account has been successfully deleted.
            string htmlContent = "<p>Hello,</p><p>Thank you for submitting a fault report.</p>" +
                "<p>DFI staff will now review your report and you will receive update emails as the status of the report progress.</p>" +
                "<br/>" +
                "<p><strong>Report details:</strong></p>" +
                "<p>" + FaultType.FaultTypeDescription + "</p>" +
                "<p>" + sessionFault.RoadNumber + ", " + sessionFault.RoadName + ", " + sessionFault.RoadTown + ", " + sessionFault.RoadCounty + "</p>" +
                "<p>" + insertedReport.AdditionalInfo + "</p>";

            //Set the attachment to null as it will not be used here.
            SendGrid.Helpers.Mail.Attachment? attachment = null;

            //Call the SendEmail in the _emailService and return the response.
            return await _emailService.SendEmail(subject, to, textContent, htmlContent, attachment);
        }
        #endregion Add Report
    }
}
