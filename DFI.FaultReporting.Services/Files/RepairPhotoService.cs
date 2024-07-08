using DFI.FaultReporting.Http.Files;
using DFI.FaultReporting.Models.Files;
using DFI.FaultReporting.Services.Interfaces.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class RepairPhotoService : IRepairPhotoService
    {
        private readonly RepairPhotoHttp _repairPhotoHttp;

        public List<RepairPhoto>? RepairPhotos { get; set; }

        public RepairPhotoService(RepairPhotoHttp repairPhotoHttp)
        {
            _repairPhotoHttp = repairPhotoHttp;
        }

        public async Task<List<RepairPhoto>> GetRepairPhotos(string token)
        {
            RepairPhotos = await _repairPhotoHttp.GetRepairPhotos(token);

            return RepairPhotos;
        }

        public async Task<RepairPhoto> GetRepairPhoto(int ID, string token)
        {
            RepairPhoto repairPhoto = await _repairPhotoHttp.GetRepairPhoto(ID, token);

            return repairPhoto;
        }

        public async Task<RepairPhoto> CreateRepairPhoto(RepairPhoto repairPhoto, string token)
        {
            repairPhoto = await _repairPhotoHttp.CreateRepairPhoto(repairPhoto, token);

            return repairPhoto;
        }

        public async Task<RepairPhoto> UpdateRepairPhoto(RepairPhoto repairPhoto, string token)
        {
            repairPhoto = await _repairPhotoHttp.UpdateRepairPhoto(repairPhoto, token);

            return repairPhoto;
        }
    }
}
