using DFI.FaultReporting.Http.Files;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class ClaimPhotoService : IClaimPhotoService
    {
        private readonly ClaimPhotoHttp _claimPhotoHttp;

        public List<ClaimPhoto>? ClaimPhotos { get; set; }

        public ClaimPhotoService(ClaimPhotoHttp claimPhotoHttp)
        {
            _claimPhotoHttp = claimPhotoHttp;
        }

        public async Task<List<ClaimPhoto>> GetClaimPhotos(string token)
        {
            ClaimPhotos = await _claimPhotoHttp.GetClaimPhotos(token);

            return ClaimPhotos;
        }

        public async Task<ClaimPhoto> GetClaimPhoto(int ID, string token)
        {
            ClaimPhoto claimPhoto = await _claimPhotoHttp.GetClaimPhoto(ID, token);

            return claimPhoto;
        }

        public async Task<ClaimPhoto> CreateClaimPhoto(ClaimPhoto claimPhoto, string token)
        {
            claimPhoto = await _claimPhotoHttp.CreateClaimPhoto(claimPhoto, token);

            return claimPhoto;
        }

        public async Task<ClaimPhoto> UpdateClaimPhoto(ClaimPhoto claimPhoto, string token)
        {
            claimPhoto = await _claimPhotoHttp.UpdateClaimPhoto(claimPhoto, token);

            return claimPhoto;
        }
    }
}
