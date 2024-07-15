using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Tokens;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DFI.FaultReporting.Common.SessionStorage;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step7Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step7Model> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimFileService _claimFileService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step7Model(ILogger<Step7Model> logger, IUserService userService, IClaimService claimService, IClaimFileService claimFileService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService, IFileValidationService fileValidationService)
        {
            _logger = logger;
            _userService = userService;
            _claimService = claimService;
            _claimFileService = claimFileService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
            _fileValidationService = fileValidationService;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        public List<ClaimFile> ClaimFiles { get; set; }

        [Display(Name = "Claim file")]
        [DataType(DataType.Upload)]
        public IFormFile? SelectedFile { get; set; }
        #endregion Properties


        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the session is checked for any claim files that have been previously added
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    //Get the claim files from "ClaimFiles" object stored in session.
                    List<ClaimFile>? sessionClaimFiles = HttpContext.Session.GetFromSession<List<ClaimFile>>("ClaimFiles");

                    //User has previously uploaded files.
                    if (sessionClaimFiles != null && sessionClaimFiles.Count > 0)
                    {
                        ClaimFiles = sessionClaimFiles.ToList();
                    }

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

        #region Upload File
        //Method Summary:
        //This method is executed when the upload button is clicked.
        //When excuted the selected file is validated and if valid is added to the ClaimFiles list and displayed on screen for the user.
        public async Task<IActionResult> OnPostUpload()
        {
            //Get the claim files from "ClaimFiles" object stored in session.
            List<ClaimFile>? sessionClaimFiles = HttpContext.Session.GetFromSession<List<ClaimFile>>("ClaimFiles");

            //User has previously uploaded files.
            if (sessionClaimFiles != null && sessionClaimFiles.Count > 0)
            {
                ClaimFiles = sessionClaimFiles.ToList();
            }
            //User has not uploaded any files.
            else
            {
                ClaimFiles = new List<ClaimFile>();
            }

            //User has uploaded less than 5 files.
            if (ClaimFiles.Count < 5)
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
                                ModelState.AddModelError("SelectedFile", "Selected file must be in .pdf or .docx format");
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

                            //File extension is not valid.
                            if (fileExtension != ".pdf" && fileExtension != ".docx")
                            {
                                ModelState.AddModelError("SelectedFile", "Selected file must be in .pdf or .docx format");
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

                            //Create new ClaimFile.
                            ClaimFile? claimFile = new ClaimFile();
                            claimFile.ClaimID = 0;
                            claimFile.Description = fileName;
                            claimFile.Type = fileExtension;
                            claimFile.Data = Convert.ToBase64String(fileBytes);
                            claimFile.InputBy = CurrentUser.Email;
                            claimFile.InputOn = DateTime.Now;
                            claimFile.Active = true;

                            //Add the new file to the list of files.
                            ClaimFiles.Add(claimFile);

                            //Set the upload files in session list.
                            HttpContext.Session.SetInSession("ClaimFiles", ClaimFiles);

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
            //User has uploaded 5 files.
            else
            {
                //Add an error to the ModelState to inform the user of en validation errors.
                ModelState.AddModelError("SelectedFile", "You can only upload a maximum of 5 files");

                //Return the page.
                return Page();
            }
        }
        #endregion Upload File

        #region Remove File
        //Method Summary:
        //This method is executed when the trash can button is clicked beneath any of the onscreen files.
        //When executed the selected file is removed from the ClaimFiles list.
        public async Task<IActionResult> OnPostRemoveFile(string removeFileValue)
        {
            //Get the claim files from "ClaimFiles" object stored in session.
            List<ClaimFile>? sessionClaimFiles = HttpContext.Session.GetFromSession<List<ClaimFile>>("ClaimFiles");

            //User has previously uploaded files.
            if (sessionClaimFiles != null && sessionClaimFiles.Count > 0)
            {
                ClaimFiles = sessionClaimFiles.ToList();
            }
            //User has not uploaded any files.
            else
            {
                ClaimFiles = new List<ClaimFile>();
            }

            //Find the claim file to remove using the passed decription value.
            ClaimFile? reportFileToRemove = ClaimFiles.Where(cf => cf.Description == removeFileValue).FirstOrDefault();

            //The claim file to remove has been found.
            if (reportFileToRemove != null)
            {
                //Remove the file.
                ClaimFiles.Remove(reportFileToRemove);
            }

            //Set the upload claim file in session list.
            HttpContext.Session.SetInSession("ClaimFiles", ClaimFiles);

            //Return the Page.
            return Page();

        }
        #endregion Remove File

        #region Page Buttons
        //Method Summary:
        //This method is executed when the next button is clicked.
        //When executed the user is redirected to Step8.
        public async Task<IActionResult> OnPostNext()
        {
            return Redirect("/Claims/SubmitClaim/Step8");
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step6 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step6");
        }
        #endregion Page Buttons
    }
}
