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
        //Declare CurrentStaff property, this is needed when calling the _staffService.
        public Staff CurrentStaff { get; set; }

        //Declare Fault property, this is needed for assign staff to the fault.
        [BindProperty]
        public Fault Fault { get; set; }

        //Declare Contractors property, this is needed when getting all contractors from the DB.
        [BindProperty]
        public List<Contractor> Contractors { get; set; }

        //Declare ContractorToAssign property, this is needed for assign contractor to the fault.
        [BindProperty]
        public Contractor? ContractorToAssign { get; set; }

        //Declare Repairs property, this is needed when getting all repairs from the DB.
        [BindProperty]
        public List<Repair> Repairs { get; set; }

        //Declare Repair property, this is needed to schedule repair to the fault.
        [BindProperty]
        public Repair? Repair { get; set; }

        //Declare SearchString property, this is needed for searching staff.
        [DisplayName("Search for contractor")]
        [BindProperty]
        public string? SearchString { get; set; }

        //Declare SearchID property, this is needed for searching staff.
        [BindProperty]
        public int SearchID { get; set; }

        //Declare RepairStatuses property, this is needed for populating repair status dropdown list.
        [BindProperty]
        public List<RepairStatus> RepairStatuses { get; set; }

        //Declare RepairStatusList property, this is needed for populating repair status dropdown list.
        [BindProperty]
        public IEnumerable<SelectListItem> RepairStatusList { get; set; }

        //Declare UpdateSuccess property, this is needed for displaying the updated success message.
        public bool UpdateSuccess { get; set; }

        //Declare TargetDateDay property, this is needed for storing the day value from the repair target date.
        [DisplayName("Day")]
        public int TargetDateDay { get; set; }

        //Declare TargetDateMonth property, this is needed for storing the month value from the repair target date.
        [DisplayName("Month")]
        public int TargetDateMonth { get; set; }

        //Declare TargetDateYear property, this is needed for storing the year value from the repair target date.
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
                    //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
                    string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                    //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
                    Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

                    //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
                    string? jwtToken = jwtTokenClaim.Value;

                    //Set the CurrentStaff property by calling the GetUser method in the _userService.
                    CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

                    //Get fault from DB.
                    Fault = await _faultService.GetFault((int)ID, jwtToken);

                    //Get all contractors from DB.
                    Contractors = await _contractorService.GetContractors(jwtToken);

                    //Filter out inactive contractors.
                    Contractors = Contractors.Where(c => c.Active == true).ToList();

                    //Get all repair statuses from DB.
                    RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

                    //Populate repair status dropdown.
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

                    //Set Fault session values.
                    HttpContext.Session.SetInSession("Fault", Fault);

                    //Set Fault session values.
                    HttpContext.Session.SetInSession("Repair", Repair);

                    //Set Contractors session values.
                    HttpContext.Session.SetInSession("Contractors", Contractors);

                    //Return the page.
                    return Page();
                }
                else
                {
                    //Redirect user to no permission.
                    return Redirect("/NoPermission");
                }
            }
            //The contexts current user has not been authenticated.
            else
            {
                //Redirect user to no permission.
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
            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Get all repair statuses from DB.
            RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

            //Populate repair status dropdown.
            RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
            {
                Text = rs.RepairStatusDescription,
                Value = rs.ID.ToString()
            });

            //Get the Repair from session.
            Repair = HttpContext.Session.GetFromSession<Repair>("Repair");

            //Set the TargetDateDay, TargetDateMonth and TargetDateYear properties.
            TargetDateDay = Repair.RepairTargetDate.Day;
            TargetDateMonth = Repair.RepairTargetDate.Month;
            TargetDateYear = Repair.RepairTargetDate.Year;

            //User has entered a search string.
            if (SearchString != null && SearchString != string.Empty)
            {
                //Get contractors from DB.
                List<Contractor> contractors = await _contractorService.GetContractors(jwtToken);

                //Set the contractor to assign to the contractor selected in the search.
                ContractorToAssign = contractors.Where(c => c.ID == SearchID).FirstOrDefault();

                //Set the ContractorToAssign in session.
                HttpContext.Session.SetInSession("ContractorToAssign", ContractorToAssign);

                //Return the page.
                return Page();
            }
            //User has not entered a search string.
            else
            {
                //Set staff to null.
                ContractorToAssign = null;

                return Page();
            }
        }

        //Method Summary:
        //This method is executed when the "Schedule" button is clicked.
        //When executed, it inserts a repair with the assigned contractor and fault ID.
        public async Task<IActionResult> OnPostScheduleRepair()
        {
            //Get the ID from the contexts current user, needed for populating CurrentUser property from DB.
            string? userID = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            //Get the JWT token claim from the contexts current user, needed for populating CurrentUser property from DB.
            Claim? jwtTokenClaim = _httpContextAccessor.HttpContext.User.FindFirst("Token");

            //Set the jwtToken string to the JWT token claims value, needed for populating CurrentUser property from DB.
            string? jwtToken = jwtTokenClaim.Value;

            //Set the CurrentStaff property by calling the GetUser method in the _userService.
            CurrentStaff = await _staffService.GetStaff(Convert.ToInt32(userID), jwtToken);

            //Get the ContractorToAssign from session.
            ContractorToAssign = HttpContext.Session.GetFromSession<Contractor>("ContractorToAssign");

            //Get the Fault from session.
            Fault = HttpContext.Session.GetFromSession<Fault>("Fault");

            //Get the Repair from session.
            Repair = HttpContext.Session.GetFromSession<Repair>("Repair");

            //Create the repair with the contractor selected to assign.
            Repair.ContractorID = ContractorToAssign.ID;

            //Insert the repair with the assigned contractor.
            Repair insertedRepair = await _repairService.CreateRepair(Repair, jwtToken);

            //The repair has been scheduled.
            if (insertedRepair != null)
            {
                //Set the UpdateSuccess property to true.
                UpdateSuccess = true;

                //Set the TargetDateDay, TargetDateMonth and TargetDateYear properties.
                TargetDateDay = insertedRepair.RepairTargetDate.Day;
                TargetDateMonth = insertedRepair.RepairTargetDate.Month;
                TargetDateYear = insertedRepair.RepairTargetDate.Year;

                //Get all repair statuses from DB.
                RepairStatuses = await _repairStatusService.GetRepairStatuses(jwtToken);

                //Populate repair status dropdown.
                RepairStatusList = RepairStatuses.Select(rs => new SelectListItem()
                {
                    Text = rs.RepairStatusDescription,
                    Value = rs.ID.ToString()
                });

                Repair = insertedRepair;

                //Update the fault status to scheduled for repair.
                Fault.FaultStatusID = 3;
                Fault updatedFault = await _faultService.UpdateFault(Fault, jwtToken);

                //Return the page.
                return Page();
            }
            //The repair has not been scheduled.
            else
            {
                //Return an error message.
                ModelState.AddModelError(string.Empty, "An error occurred while scheduling the repair");

                //Return the page.
                return Page();
            }
        }
        #endregion Schedule Repair
    }
}
