using DFI.FaultReporting.Common.SessionStorage;
using DFI.FaultReporting.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DFI.FaultReporting.Public.Pages.Faults
{
    public class MyFaultsModel : PageModel
    {
        public void OnGet()
        {
        }

        //Method Summary:
        //This method is excuted when the "Map" link is clicked.
        //When executed the "Map" section is displayed.
        public void OnGetShowMapView()
        {
            //Clear all TempData to ensure a fresh start.
            TempData.Clear();
        }

        //Method Summary:
        //This method is excuted when the "Table" link is clicked.
        //When executed the "Table" section is displayed.
        public void OnGetShowTableView()
        {
            //Clear all TempData to ensure a fresh start.
            TempData.Clear();
        }
    }
}
