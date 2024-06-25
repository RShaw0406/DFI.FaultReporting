using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Admin;
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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.DrawingCore;
using System.Security.Claims;

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
        private readonly IReportPhotoService _reportPhotoService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step3Model(ILogger<Step3Model> logger, IUserService userService, IReportService reportService, IFaultService faultService, IReportPhotoService reportPhotoService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService, IFileValidationService fileValidationService)
        {
            _logger = logger;
            _userService = userService;
            _faultService = faultService;
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

        //Declare ReportPhotos property, this is needed to store uploaded photos.
        public List<ReportPhoto> ReportPhotos {  get; set; }

        //Declare SelectedFile property, this is needed for storing individual photo each time one is uploaded.
        [Display(Name = "Fault photo")]
        [DataType(DataType.Upload)]
        public IFormFile? SelectedFile { get; set; }
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
                    //Get the fault from "Fault" object stored in session.
                    Fault? sessionFault = HttpContext.Session.GetFromSession<Fault>("Fault");

                    //Get the report from "Report" object stored in session.
                    Report? sessionReport = HttpContext.Session.GetFromSession<Report>("Report");

                    //Get the report photos from "ReportPhotos" object stored in session.
                    List<ReportPhoto>? sessionReportPhotos = HttpContext.Session.GetFromSession<List<ReportPhoto>>("ReportPhotos");

                    //User has previously uploaded images.
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

        #region Step3
        //Method Summary:
        //This method is executed when the upload button is clicked.
        //When excuted the selected file is validated and if valid is added to the ReportPhotos list and displayed on screen for the user.
        public async Task<IActionResult> OnPostUpload()
        {
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
        //This method is executed when the next button is clicked.
        //When executed the user is redirected to Step4.
        public async Task<IActionResult> OnPostNext()
        {
            return Redirect("/Faults/ReportFault/Step4");
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step2.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Faults/ReportFault/Step2");
        }
        #endregion Step3
    }
}
