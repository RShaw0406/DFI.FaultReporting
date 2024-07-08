using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Files
{
    public interface IRepairPhotoSQLRepository
    {
        Task<List<RepairPhoto>> GetRepairPhotos();

        Task<RepairPhoto> GetRepairPhoto(int ID);

        Task<RepairPhoto> CreateRepairPhoto(RepairPhoto repairPhoto);

        Task<RepairPhoto> UpdateRepairPhoto(RepairPhoto repairPhoto);
    }
}
