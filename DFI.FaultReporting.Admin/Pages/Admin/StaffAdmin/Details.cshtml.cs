using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DFI.FaultReporting.Models.Users;
using DFI.FaultReporting.SQL.Repository.Contexts;

namespace DFI.FaultReporting.Admin.Pages.Admin.StaffAdmin
{
    public class DetailsModel : PageModel
    {
        private readonly DFI.FaultReporting.SQL.Repository.Contexts.DFIFaultReportingDataContext _context;

        public DetailsModel(DFI.FaultReporting.SQL.Repository.Contexts.DFIFaultReportingDataContext context)
        {
            _context = context;
        }

        public Staff Staff { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff.FirstOrDefaultAsync(m => m.ID == id);
            if (staff == null)
            {
                return NotFound();
            }
            else
            {
                Staff = staff;
            }
            return Page();
        }
    }
}
