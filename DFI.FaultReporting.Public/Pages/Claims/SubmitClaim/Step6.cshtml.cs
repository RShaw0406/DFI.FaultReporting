using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Interfaces.FaultReports;
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
using DFI.FaultReporting.Common.SessionStorage;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Claims.SubmitClaim
{
    public class Step6Model : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<Step6Model> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimPhotoService _claimPhotoService;
        private readonly IFileValidationService _fileValidationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IEmailService _emailService;
        private readonly IVerificationTokenService _verificationTokenService;

        //Inject dependencies in constructor.
        public Step6Model(ILogger<Step6Model> logger, IUserService userService, IClaimService claimService, IClaimPhotoService claimPhotoService,
            IHttpContextAccessor httpContextAccessor, ISettingsService settingsService, IEmailService emailService,
            IVerificationTokenService verificationTokenService, IFileValidationService fileValidationService)
        {
            _logger = logger;
            _userService = userService;
            _claimService = claimService;
            _claimPhotoService = claimPhotoService;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _emailService = emailService;
            _verificationTokenService = verificationTokenService;
            _fileValidationService = fileValidationService;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        public List<ClaimPhoto> ClaimPhotos { get; set; }

        [Display(Name = "Claim photo")]
        [DataType(DataType.Upload)]
        public IFormFile? SelectedFile { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the session is checked for any claim photos that have been previously added
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    //Get the claim photos from "ClaimPhotos" object stored in session.
                    List<ClaimPhoto>? sessionClaimPhotos = HttpContext.Session.GetFromSession<List<ClaimPhoto>>("ClaimPhotos");

                    //User has previously uploaded images.
                    if (sessionClaimPhotos != null && sessionClaimPhotos.Count > 0)
                    {
                        ClaimPhotos = sessionClaimPhotos.ToList();
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

        #region Upload Photo
        //Method Summary:
        //This method is executed when the upload button is clicked.
        //When excuted the selected file is validated and if valid is added to the ClaimPhotos list and displayed on screen for the user.
        public async Task<IActionResult> OnPostUpload()
        {
            //Get the claim photos from "ClaimPhotos" object stored in session.
            List<ClaimPhoto>? sessionClaimPhotos = HttpContext.Session.GetFromSession<List<ClaimPhoto>>("ClaimPhotos");

            //User has previously uploaded images.
            if (sessionClaimPhotos != null && sessionClaimPhotos.Count > 0)
            {
                ClaimPhotos = sessionClaimPhotos.ToList();
            }
            //User has not uploaded any images.
            else
            {
                ClaimPhotos = new List<ClaimPhoto>();
            }

            //User has uploaded less than 5 photos.
            if (ClaimPhotos.Count < 5)
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

                            //Create new ClaimPhoto.
                            ClaimPhoto? claimPhoto = new ClaimPhoto();
                            claimPhoto.ClaimID = 0;
                            claimPhoto.Description = fileName;
                            claimPhoto.Type = fileExtension;
                            claimPhoto.Data = Convert.ToBase64String(fileBytes);
                            claimPhoto.InputBy = CurrentUser.Email;
                            claimPhoto.InputOn = DateTime.Now;
                            claimPhoto.Active = true;

                            //Add the new photo to the list of photos.
                            ClaimPhotos.Add(claimPhoto);

                            //Set the upload photos in session list.
                            HttpContext.Session.SetInSession("ClaimPhotos", ClaimPhotos);

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
        #endregion Upload Photo

        #region Remove Photo
        //Method Summary:
        //This method is executed when the trash can button is clicked beneath any of the onscreen photos.
        //When executed the selected photo is removed from the ClaimPhotos list.
        public async Task<IActionResult> OnPostRemovePhoto(string removePhotoValue)
        {
            //Get the claim photos from "ClaimPhotos" object stored in session.
            List<ClaimPhoto>? sessionClaimPhotos = HttpContext.Session.GetFromSession<List<ClaimPhoto>>("ClaimPhotos");

            //User has previously uploaded images.
            if (sessionClaimPhotos != null && sessionClaimPhotos.Count > 0)
            {
                ClaimPhotos = sessionClaimPhotos.ToList();
            }
            //User has not uploaded any images.
            else
            {
                ClaimPhotos = new List<ClaimPhoto>();
            }

            //Find the claim photo to remove using the passed decription value.
            ClaimPhoto? claimPhotoToRemove = ClaimPhotos.Where(cp => cp.Description == removePhotoValue).FirstOrDefault();

            //The claim photo to remove has been found.
            if (claimPhotoToRemove != null)
            {
                //Remove the photo.
                ClaimPhotos.Remove(claimPhotoToRemove);
            }

            //Set the upload claim photo in session list.
            HttpContext.Session.SetInSession("ClaimPhotos", ClaimPhotos);

            //Return the Page.
            return Page();

        }
        #endregion Remove Photo

        #region Page Buttons
        //Method Summary:
        //This method is executed when the next button is clicked.
        //When executed the user is redirected to Step7.
        public async Task<IActionResult> OnPostNext()
        {
            return Redirect("/Claims/SubmitClaim/Step7");
        }

        //Method Summary:
        //This method is executed when the back button is clicked.
        //When executed the user is redirected to Step5 page.
        public async Task<IActionResult> OnPostBack()
        {
            return Redirect("/Claims/SubmitClaim/Step5");
        }
        #endregion Page Buttons
    }
}
