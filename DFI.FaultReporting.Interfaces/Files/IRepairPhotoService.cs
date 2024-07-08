using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Interfaces.Files
{
    public interface IRepairPhotoService
    {
        Task<List<RepairPhoto>> GetRepairPhotos(string token);

        Task<RepairPhoto> GetRepairPhoto(int ID, string token);

        Task<RepairPhoto> CreateRepairPhoto(RepairPhoto repairPhoto, string token);

        Task<RepairPhoto> UpdateRepairPhoto(RepairPhoto repairPhoto, string token);
    }
}
