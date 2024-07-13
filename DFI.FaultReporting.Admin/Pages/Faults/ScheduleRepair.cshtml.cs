using DFI.FaultReporting.Interfaces.Admin;
using DFI.FaultReporting.Interfaces.FaultReports;
using DFI.FaultReporting.Models.Admin;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.Services.Interfaces.Admin;
using DFI.FaultReporting.Services.Interfaces.Pagination;
using DFI.FaultReporting.Services.Interfaces.Settings;
using DFI.FaultReporting.Services.Interfaces.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.Security.Claims;
using DFI.FaultReporting.Common.SessionStorage;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DFI.FaultReporting.Admin.Pages.Faults
{
    public class ScheduleRepairModel : PageModel
    {
        #region Dependency Injection
        //Declare dependencies.
        private readonly ILogger<ScheduleRepairModel> _logger;
        private readonly IStaffService _staffService;
        private readonly IContractorService _contractorService;
        private readonly IFaultService _faultService;
        private readonly IFaultPriorityService _faultPriorityService;
        private readonly IFaultStatusService _faultStatusService;
        private readonly IFaultTypeService _faultTypeService;
        private readonly IRepairService _repairService;
        private readonly IRepairStatusService _repairStatusService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPagerService _pagerService;
        private readonly ISettingsService _settingsService;

        //Inject dependencies in constructor.
        public ScheduleRepairModel(ILogger<ScheduleRepairModel> logger, IStaffService staffService, IContractorService contractorService, IFaultService faultService, 
            IFaultTypeService faultTypeService, IFaultPriorityService faultPriorityService, IFaultStatusService faultStatusService, IRepairService repairService,
            IRepairStatusService repairStatusService,  IHttpContextAccessor httpContextAccessor, IPagerService pagerService, ISettingsService settingsService)
        {
            _logger = logger;
            _staffService = staffService;
            _contractorService = contractorService;
            _faultService = faultService;
            _faultPriorityService = faultPriorityService;
            _faultStatusService = faultStatusService;
            _faultTypeService = faultTypeService;
            _repairService = repairService;
            _repairStatusService = repairStatusService;
            _httpContextAccessor = httpContextAccessor;
            _pagerService = pagerService;
            _settingsService = settingsService;
        }
        #endregion Dependency Injection

        #region Properties
        public Staff CurrentStaff { get; set; }

        [BindProperty]
        public Fault Fault { get; set; }

        [BindProperty]
        public List<Contractor> Contractors { get; set; }

        [BindProperty]
        public Contractor? ContractorToAssign { get; set; }

        [BindProperty]
        public List<Repair> Repairs { get; set; }

        [BindProperty]
        public Repair? Repair { get; set; }

        [DisplayName("Search for contractor")]
        [BindProperty]
        public string? SearchString { get; set; }

        [BindProperty]
        public int SearchID { get; set; }

        [BindProperty]
        public List<RepairStatus> RepairStatuses { get; set; }

        [BindProperty]
        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        public bool UpdateSuccess { get; set; }

        [DisplayName("Day")]
        public int TargetDateDay { get; set; }

        [DisplayName("Month")]
        public int TargetDateMonth { get; set; }

        [DisplayName("Year")]
        public int TargetDateYear { get; set; }
        #endregion Properties

        #region Page Load
        //Method Summary:
        //This method is called when the page is loaded.
        //It checks if the current user is authenticated and if so, it gets the fault details from the DB.
        public async Task<IActionResult> OnGetAsync(int? ID)
        {
            //The contexts current user exists.
            if (_httpContextAccessor.HttpContext.User != null)
            {
                //The contexts current user has been authenticated and has admin role.
                if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated == true && HttpContext.User.IsInRole("StaffReadWrite"))
                {
                    await PopulateProperties((int)ID);

                    //Set the ID in TempData.
                    TempData["ID"] = ID;
                    TempData.Keep();

                    //Set Contractors session value, this is needed for the javascript search.
                    HttpContext.Session.SetInSession("Contractors", Contractors);

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

        #region Schedule Repair
        //Method Summary:
        //This method is executed when a search item clicked.
        //When executed, it sets the contractor to assign to the contractor selected in the search.
        public async Task<IActionResult> OnPostAssignToSearch()
        {
            await PopulateProperties((int)TempData["ID"]);

            //User has entered a search string.
            if (SearchString != null && SearchString != string.Empty)
            {
                //Get contractors from DB.
                Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
                string? jwtToken = jwtTokenClaim.Value;
                List<Contractor> contractors = await _contractorService.GetContractors(jwtToken);

                //Set the contractor to assign to the contractor selected in the search.
                ContractorToAssign = contractors.Where(c => c.ID == SearchID).FirstOrDefault();

                //Set the ContractorToAssign in session.
                HttpContext.Session.SetInSession("ContractorToAssign", ContractorToAssign);

                TempData.Keep();

                return Page();
            }
            else
            {
                ContractorToAssign = null;

                TempData.Keep();

                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Schedule" button is clicked.
        //When executed, it inserts a repair with the assigned contractor and fault ID.
        public async Task<IActionResult> OnPostScheduleRepair()
        {
            await PopulateProperties((int)TempData["ID"]);

            //Get the ContractorToAssign from session.
            ContractorToAssign = HttpContext.Session.GetFromSession<Contractor>("ContractorToAssign");

            //Create the repair with the contractor selected to assign.
            Repair.ContractorID = ContractorToAssign.ID;

            //Insert the repair with the assigned contractor.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            Repair insertedRepair = await _repairService.CreateRepair(Repair, jwtToken);

            //The repair has been scheduled.
            if (insertedRepair != null)
            {
                UpdateSuccess = true;

                //Set the TargetDateDay, TargetDateMonth and TargetDateYear properties.
                TargetDateDay = insertedRepair.RepairTargetDate.Day;
                TargetDateMonth = insertedRepair.RepairTargetDate.Month;
                TargetDateYear = insertedRepair.RepairTargetDate.Year;

                Repair = insertedRepair;

                //Update the fault status to scheduled for repair.
                Fault.FaultStatusID = 3;
                Fault updatedFault = await _faultService.UpdateFault(Fault, jwtToken);

                return Page();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while scheduling the repair");

                return Page();
            }
        }
        #endregion Schedule Repair

        #region Data
        //Method Summary:
        //This method is excuted when the a post occurs.
        //When excuted, it populates the page properties.
        public async Task PopulateProperties(int ID)
        {
            //Get the current user ID and JWT token.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");
            string? jwtToken = jwtTokenClaim.Value;
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            Fault = await _faultService.GetFault((int)ID, jwtToken);

            Contractors = await _contractorService.GetContractors(jwtToken);

            Contractors = Contractors.Where(c => c.Active == true).ToList();

            RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            //Get all fault priorities from DB.
            List<FaultPriority> faultPriorities = await _faultPriorityService.GetFaultPriorities();

            //Get the fault priority from the FaultPriority list where the ID is equal to the Fault.FaultPriorityID.
            FaultPriority? faultPriority = faultPriorities.Where(fp => fp.ID == Fault.FaultPriorityID).FirstOrDefault();

            //Declare repair object.
            Repair = new Repair();
            Repair.FaultID = Fault.ID;

            //Fault priority is not null.
            if (faultPriority != null)
            {
                //Fault priority is R0 so make the target date the same day as fault was reported.
                if (faultPriority.ID == 4)
                {
                    //Set the repair target date to the same day as the fault was reported.
                    Repair.RepairTargetDate = Fault.InputOn;
                }
                //Fault priority is R1 so make the target date the next calender day.
                else if (Fault.FaultPriorityID == 7)
                {
                    //Set the repair target date to the next calender day.
                    Repair.RepairTargetDate = Fault.InputOn.AddDays(1);
                }
                //Fault priority is R2 so make the target date the next working day within 5 days.
                else if (Fault.FaultPriorityID == 8)
                {
                    //Add 5 days to the fault input date.
                    Repair.RepairTargetDate = Fault.InputOn.AddDays(5);

                    //The repair target date is a Saturday.
                    if (Repair.RepairTargetDate.DayOfWeek == DayOfWeek.Saturday)
                    {
                        //Add 2 days to the repair target date.
                        Repair.RepairTargetDate = Repair.RepairTargetDate.AddDays(2);
                    }
                    //The repair target date is a Sunday.
                    else if (Repair.RepairTargetDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        //Add 1 day to the repair target date.
                        Repair.RepairTargetDate = Repair.RepairTargetDate.AddDays(1);
                    }
                }
                //Fault priority is R3 so make the target date the next working day within 28 days.
                else if (Fault.FaultPriorityID == 9)
                {
                    //Add 28 days to the fault input date.
                    Repair.RepairTargetDate = Fault.InputOn.AddDays(28);
                }
                //Any other fault priority set target date to within 1 year, this is within the yearly inspection cycle.
                else
                {
                    //Add 365 days to the fault input date.
                    Repair.RepairTargetDate = Fault.InputOn.AddDays(365);
                }
            }

            Repair.ActualRepairDate = null;
            Repair.RepairNotes = null;
            //Set repair status to scheduled.
            Repair.RepairStatusID = 5;
            Repair.ContractorID = 0;
            Repair.InputBy = CurrentStaff.Email;
            Repair.InputOn = DateTime.Now;
            Repair.Active = true;

            //Set the TargetDateDay, TargetDateMonth and TargetDateYear properties.
            TargetDateDay = Repair.RepairTargetDate.Day;
            TargetDateMonth = Repair.RepairTargetDate.Month;
            TargetDateYear = Repair.RepairTargetDate.Year;
        }
        #endregion Data
    }
}
