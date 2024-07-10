using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Files;
using DFI.FaultReporting.Services.Interfaces.Users;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DFI.FaultReporting.Public.Pages.Jobs
{
    public class AddImageModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<AddImageModel> _logger;
        private readonly IUserService _userService;
        private readonly IRepairService _repairService;
        private readonly IRepairPhotoService _repairPhotoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileValidationService _fileValidationService;

        //Inject dependencies in constructor.
        public AddImageModel(ILogger<AddImageModel> logger, IUserService userService, IRepairService repairService, IRepairPhotoService repairPhotoService,
            IHttpContextAccessor httpContextAccessor, IFileValidationService fileValidationService)
        {
            _logger = logger;
            _userService = userService;
            _repairService = repairService;
            _repairPhotoService = repairPhotoService;
            _httpContextAccessor = httpContextAccessor;
            _fileValidationService = fileValidationService;
        }
        #endregion Dependency Injection

        #region Properties
        public User CurrentUser { get; set; }

        [BindProperty]
        public Repair Repair { get; set; }

        [BindProperty]
        public List<RepairPhoto> RepairPhotos { get; set; }

        //Declare SelectedFile property, this is needed for storing individual photo each time one is uploaded.
        [Display(Name = "Repair photo")]
        [DataType(DataType.Upload)]
        public IFormFile? SelectedFile { get; set; }
        #endregion Properties


        #region Page Load
        //Method Summary:
        //This method is executed when the page loads.
        //When executed the session is checked for any repair photos that have been previously added
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("Contractor"))
                {
                    //Clear session and tempdata to ensure fresh start.
                    HttpContext.Session.Clear();
                    TempData.Clear();

                    await PopulateProperties(ID);

                    //Store the repair ID in TempData, this is needed populate page properties, when the form is submitted.
                    TempData["RepairID"] = Repair.ID;
                    TempData.Keep();

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
        //When excuted the selected file is validated and if valid is added to the ReportPhotos list and displayed on screen for the user.
        public async Task<IActionResult> OnPostUpload()
        {
            //Get the repair ID from TempData.
            int RepairID = Convert.ToInt32(TempData["RepairID"]);
            TempData.Keep();

            await PopulateProperties(RepairID);

            List<RepairPhoto>? sessionRepairPhotos = HttpContext.Session.GetFromSession<List<RepairPhoto>>("RepairPhotos");

            //User has previously uploaded images.
            if (sessionRepairPhotos != null && sessionRepairPhotos.Count > 0)
            {
                RepairPhotos = sessionRepairPhotos.ToList();
            }
            else
            {
                RepairPhotos = new List<RepairPhoto>();
            }

            //User has uploaded less than 5 photos.
            if (RepairPhotos.Count < 5)
            {
                if (SelectedFile != null)
                {
                    ValidationContext validationContext = new ValidationContext(SelectedFile);

                    ICollection<ValidationResult> validationResults = new List<ValidationResult>();

                    bool isSelectedFileValid = Validator.TryValidateObject(SelectedFile, validationContext, validationResults, true);

                    //SelectedFile is valid.
                    if (isSelectedFileValid == true)
                    {
                        if (SelectedFile.Length > 0)
                        {
                            bool fileIsValid = await _fileValidationService.ValidateDocument(SelectedFile);

                            //File is not .jpg, .jpeg, .png.
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

                            string fileName = Path.GetFileName(SelectedFile.FileName);

                            string fileExtension = Path.GetExtension(fileName);

                            //File is not .jpg, .jpeg, .png.
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

                            //Create new RepairPhoto.
                            RepairPhoto repairPhoto = new RepairPhoto();
                            repairPhoto.RepairID = Repair.ID;
                            repairPhoto.Description = fileName;
                            repairPhoto.Type = fileExtension;
                            repairPhoto.Data = Convert.ToBase64String(fileBytes);
                            repairPhoto.InputBy = CurrentUser.Email;
                            repairPhoto.InputOn = DateTime.Now;
                            repairPhoto.Active = true;

                            //Add the new photo to the list of photos.
                            RepairPhotos.Add(repairPhoto);

                            //Set the upload report in session list.
                            HttpContext.Session.SetInSession("RepairPhotos", RepairPhotos);

                            return Page();

                        }
                        //File size is 0 bytes.
                        else
                        {
                            ModelState.AddModelError("SelectedFile", "The selected file does not contain any data");

                            return Page();
                        }
                    }
                    //SelectedFile is not valid.
                    else
                    {
                        foreach (ValidationResult validationResult in validationResults)
                        {
                            ModelState.AddModelError("SelectedFile", validationResult.ErrorMessage);
                        }

                        return Page();
                    }
                }
                //No file has been selected in upload control.
                else
                {
                    ModelState.AddModelError("SelectedFile", "You have not selected any file to upload");

                    return Page();
                }
            }
            //User has uploaded 5 photos.
            else
            {
                ModelState.AddModelError("SelectedFile", "You can only upload a maximum of 5 photos");

                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the trash can button is clicked beneath any of the onscreen photos.
        //When executed the selected photo is removed from the RepairPhotos list.
        public async Task<IActionResult> OnPostRemovePhoto(string removePhotoValue)
        {
            //Get the repair ID from TempData.
            int RepairID = Convert.ToInt32(TempData["RepairID"]);
            TempData.Keep();

            await PopulateProperties(RepairID);

            //Get the repair photos from "RepairPhotos" object stored in session.
            List<RepairPhoto>? sessionRepairPhotos = HttpContext.Session.GetFromSession<List<RepairPhoto>>("RepairPhotos");

            //User has previously uploaded images.
            if (sessionRepairPhotos != null && sessionRepairPhotos.Count > 0)
            {
                RepairPhotos = sessionRepairPhotos.ToList();
            }
            else
            {
                RepairPhotos = new List<RepairPhoto>();
            }

            //Find the repair photo to remove using the passed decription value.
            RepairPhoto? repairPhotoToRemove = RepairPhotos.Where(rp => rp.Description == removePhotoValue).FirstOrDefault();

            if (repairPhotoToRemove != null)
            {
                //Remove the photo.
                RepairPhotos.Remove(repairPhotoToRemove);
            }

            HttpContext.Session.SetInSession("RepairPhotos", RepairPhotos);

            return Page();
        }

        //Method Summary:
        //This method is executed when the submit button is clicked.
        //When executed the repair photos in the session are submitted to the database.
        public async Task<IActionResult> OnPostSubmitPhotos()
        {
            //Get the repair ID from TempData.
            int RepairID = Convert.ToInt32(TempData["RepairID"]);

            await PopulateProperties(RepairID);

            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;

            //Get the repair photos from "RepairPhotos" object stored in session.
            List<RepairPhoto>? sessionRepairPhotos = HttpContext.Session.GetFromSession<List<RepairPhoto>>("RepairPhotos");

            if (sessionRepairPhotos != null && sessionRepairPhotos.Count > 0)
            {
                foreach (RepairPhoto repairPhoto in sessionRepairPhotos)
                {
                    RepairPhoto? insertedRepairPhoto = await _repairPhotoService.CreateRepairPhoto(repairPhoto, jwtToken);

                    if (insertedRepairPhoto == null)
                    {
                        ModelState.AddModelError(string.Empty, "An error occure when inserting the repair photo");

                        return Page();
                    }
                }
            }

            return Redirect("/Jobs/JobImages?ID=" + RepairID);
        }
        #endregion Upload Photo

        #region Data
        //Method Summary:
        //This method is excuted when the page loads.
        //When executed, it populates the page properties.
        public async Task PopulateProperties(int? ID)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            Repair = await _repairService.GetRepair(Convert.ToInt32(ID), jwtToken);
        }
        #endregion Data
    }
}
