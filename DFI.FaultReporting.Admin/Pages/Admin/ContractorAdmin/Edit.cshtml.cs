using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Emails;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using System.Security.Claims;

namespace DFI.FaultReporting.Admin.Pages.Admin.ContractorAdmin
{
    public class EditModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<EditModel> _logger;
        private readonly IContractorService _contractorService;
        private readonly IStaffService _staffService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public EditModel(ILogger<EditModel> logger, IContractorService contractorService, IStaffService staffService, IHttpContextAccessor httpContextAccessor,
            IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _contractorService = contractorService;
            _staffService = staffService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Contractor Contractor { get; set; }

        public bool UpdateSuccess { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the contractor details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffAdmin"))
                {
                    //Clear session to ensure fresh start.
                    HttpContext.Session.Clear();

                    await PopulateProperties();

                    //Get ccontractor from the DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                    string? jwtToken = jwtTokenClaim.Value;
                    Contractor = await _contractorService.GetContractor((int)ID, jwtToken);

                    //Store the contractor email in TempData.
                    TempData["Email"] = Contractor.Email;
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

        #region Edit Contractor
        //Method Summary:
        //This method is executed when the update button is clicked.
        //When executed, it updates the contractor in the DB.
        public async Task<IActionResult> OnPostAsync()
        {
            //Modelstate is not valid.
            if (!ModelState.IsValid)
            {
                //Display each of the model state errors.
                foreach (var item in ModelState)
                {
                    if (item.Value.Errors.Count > 0)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.ErrorMessage);
                        }
                    }
                }

                return Page();
            }
            //Modelstate is valid.
            else
            {
                await PopulateProperties();

                //Get all contractors from the DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<Contractor> contractors = await _contractorService.GetContractors(jwtToken);

                //Contractor email has has changed.
                if (Contractor.Email != TempData["Email"].ToString())
                {
                    //Contractor email already exists.
                    if (contractors.Any(c => c.Email == Contractor.Email))
                    {
                        TempData.Keep();

                        ModelState.AddModelError(string.Empty, "Contractor email already exists");
                        ModelState.AddModelError("Contractor.Email", "Contractor email already exists");

                        return Page();
                    }
                }

                //Update contractor in the DB.
                Contractor updatedContractor = await _contractorService.UpdateContractor(Contractor, jwtToken);

                //Contractor was updated.
                if (updatedContractor != null)
                {
                    UpdateSuccess = true;

                    TempData["Email"] = updatedContractor.Email;

                    TempData.Keep();

                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Contractor could not be updated");

                    TempData.Keep();

                    return Page();
                }
            }
        }
        #endregion Edit Contractor

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);
        }
        #endregion Data
    }
}
