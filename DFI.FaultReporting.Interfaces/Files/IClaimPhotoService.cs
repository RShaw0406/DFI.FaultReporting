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
        Task<List<ClaimPhoto>> GetClaimPhotos(string token);

        Task<ClaimPhoto> GetClaimPhoto(int ID, string token);

        Task<ClaimPhoto> CreateClaimPhoto(ClaimPhoto claimPhoto, string token);

        Task<ClaimPhoto> UpdateClaimPhoto(ClaimPhoto claimPhoto, string token);
    }
}
