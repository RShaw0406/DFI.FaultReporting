using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.SQL.Repository.Interfaces.Files
{
    public interface IClaimPhotoSQLRepository
    {
        Task<List<ClaimPhoto>> GetClaimPhotos();

        Task<ClaimPhoto> GetClaimPhoto(int ID);

        Task<ClaimPhoto> CreateClaimPhoto(ClaimPhoto claimPhoto);

        Task<ClaimPhoto> UpdateClaimPhoto(ClaimPhoto claimPhoto);
    }
}
