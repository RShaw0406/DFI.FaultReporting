using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Files
{
    public interface IClaimPhotoService
    {
        Task<List<ClaimPhoto>> GetClaimPhotos();

        Task<ClaimPhoto> GetClaimPhoto(int ID);

        Task<ClaimPhoto> CreateClaimPhoto(ClaimPhoto claimPhoto);

        Task<ClaimPhoto> UpdateClaimPhoto(ClaimPhoto claimPhoto);

        Task<ClaimPhoto> DeleteClaimPhoto(int ID);
    }
}
