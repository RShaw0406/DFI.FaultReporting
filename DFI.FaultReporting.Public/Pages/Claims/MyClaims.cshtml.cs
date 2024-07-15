using DFI.FaultReporting.Common.Pagination;
using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.Claims;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.Claims;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Admin;
using DFI.FaultReporting.Services.Interfaces.Claims;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using DFI.FaultReporting.Services.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace DFI.FaultReporting.Public.Pages.Claims
{
    public class MyClaimsModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<MyClaimsModel> _logger;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private readonly IClaimTypeService _claimTypeService;
        private readonly ILegalRepService _legalRepService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClaimStatusService _claimStatusService;
        private readonly IWitnessService _witnessService;
        private readonly IPagerService _pagerService;

        //Inject dependencies in constructor.
        public MyClaimsModel(ILogger<MyClaimsModel> logger, IUserService userService, IClaimService claimService, IClaimTypeService claimTypeService,
            ISettingsService settingsService, IHttpContextAccessor httpContextAccessor, IClaimStatusService claimStatusService, 
            IWitnessService witnessService, ILegalRepService legalRepService, IPagerService pagerService)
        {
            _logger = logger;
            _userService = userService;
            _claimService = claimService;
            _claimTypeService = claimTypeService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _claimStatusService = claimStatusService;
            _witnessService = witnessService;
            _legalRepService = legalRepService;
            _pagerService = pagerService;
        }
        #endregion Dependency Injection

        #region Properties

        public User CurrentUser { get; set; }

        public List<Claim> Claims { get; set; }

        public List<Claim> PagedClaims { get; set; }

        [BindProperty(SupportsGet = true)]
        public Pager Pager { get; set; } = new Pager();

        public List<ClaimType> ClaimTypes { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> ClaimTypesList { get; set; }

        [DisplayName("Claim type")]
        [BindProperty]
        public int ClaimTypeFilter { get; set; }

        public List<ClaimStatus> ClaimStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> ClaimStatusList { get; set; }

        [DisplayName("Claim status")]
        [BindProperty]
        public int ClaimStatusFilter { get; set; }

        public List<Witness> Witnesses { get; set; }

        public List<LegalRep> LegalReps { get; set; }

        public bool UserHasClaims { get; set; }

        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //When executed the current user is authenticated and the properties are set.
        public async Task<IActionResult> OnGetAsync()
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has user role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && _httpContextAccessor.HttpContext.User.IsInRole("User"))
                {
                    await PopulateProperties();

                    if (Claims.Count > 0)
                    {
                        UserHasClaims = true;
                    }
                    else
                    {
                        UserHasClaims = false;
                    }

                    //Setup Pager.
                    Pager.CurrentPage = 1;
                    Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
                    Pager.Count = Claims.Count;
                    PagedClaims = await _pagerService.GetPaginatedClaims(Claims, Pager.CurrentPage, Pager.PageSize);

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

        #region Filtering
        //This method is executed when either of the dropdown filters are changed.
        //When executed the Faults property is filtered based on the selected filter.
        public async Task<IActionResult> OnPostFilter()
        {
            await PopulateProperties();

            if (Claims.Count > 0)
            {
                UserHasClaims = true;
            }
            else
            {
                UserHasClaims = false;
            }

            //-------------------- TYPE FILTER --------------------
            //User has selected an type that is not "All".
            if (ClaimTypeFilter != 0)
            {
                Claims = Claims.Where(c => c.ClaimTypeID == ClaimTypeFilter).ToList();
            }

            //-------------------- STATUS FILTER --------------------
            //User has selected a status that is not "All".
            if (ClaimStatusFilter != 0)
            {
                Claims = Claims.Where(c => c.ClaimStatusID == ClaimStatusFilter).ToList();
            }

            //Setup Pager.
            Pager.CurrentPage = 1;
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            Pager.Count = Claims.Count;
            PagedClaims = await _pagerService.GetPaginatedClaims(Claims, Pager.CurrentPage, Pager.PageSize);

            //Set the selected filters in temp data.
            TempData["ClaimTypeFilter"] = ClaimTypeFilter;
            TempData["ClaimStatusFilter"] = ClaimStatusFilter;
            TempData.Keep();

            return Page();
        }
        #endregion Filtering

        #region Pagination
        //Method Summary:
        //This method is excuted when the pagination buttons are clicked.
        //When executed the desired page of claims is displayed.
        public async Task OnGetPaging()
        {
            await PopulateProperties();

            if (Claims.Count > 0)
            {
                UserHasClaims = true;
            }
            else
            {
                UserHasClaims = false;
            }

            //User has selected a claim type.
            if (TempData["ClaimTypeFilter"] != null && (int)TempData["ClaimTypeFilter"] != 0)
            {
                ClaimTypeFilter = int.Parse(TempData["ClaimTypeFilter"].ToString());

                Claims = Claims.Where(c => c.ClaimTypeID == ClaimTypeFilter).ToList();
            }

            //User has selected a claim status.
            if (TempData["ClaimStatusFilter"] != null && (int)TempData["ClaimStatusFilter"] != 0)
            {
                ClaimStatusFilter = int.Parse(TempData["ClaimStatusFilter"].ToString());

                Claims = Claims.Where(c => c.ClaimStatusID == ClaimStatusFilter).ToList();
            }

            //Keep the TempData.
            TempData.Keep();

            //Setup Pager.
            Pager.PageSize = await _settingsService.GetSettingInt(DFI.FaultReporting.Common.Constants.Settings.PAGESIZE);
            Pager.Count = Claims.Count;
            PagedClaims = await _pagerService.GetPaginatedClaims(Claims, Pager.CurrentPage, Pager.PageSize);
        }
        #endregion Pagination

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties()
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;
            System.Security.Claims.Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentUser = await _userService.GetUser(Convert.ToInt32(userID), jwtToken);

            Claims = await _claimService.GetClaims(jwtToken);
            Claims = Claims.Where(c => c.UserID == CurrentUser.ID).ToList();

            ClaimTypes = await _claimTypeService.GetClaimTypes(jwtToken);
            ClaimTypesList = ClaimTypes.Select(ct => new SelectListItem
            {
                Value = ct.ID.ToString(),
                Text = ct.ClaimTypeDescription
            });

            ClaimStatuses = await _claimStatusService.GetClaimStatuses(jwtToken);
            ClaimStatusList = ClaimStatuses.Select(cs => new SelectListItem
            {
                Value = cs.ID.ToString(),
                Text = cs.ClaimStatusDescription
            });

            Witnesses = await _witnessService.GetWitnesses(jwtToken);
            LegalReps = await _legalRepService.GetLegalReps(jwtToken);
        }
        #endregion Data
    }
}
