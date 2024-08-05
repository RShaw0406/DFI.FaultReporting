using DFI.FaultReporting.Http.FaultReports;
using DFI.FaultReporting.Http.Files;
using DFI.FaultReporting.Interfaces.Files;
using DFI.FaultReporting.Models.FaultReports;
using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class ReportPhotoService : IReportPhotoService
    {
        private readonly ReportPhotoHttp _reportPhotoHttp;

        public List<ReportPhoto>? ReportPhotos { get; set; }

        public ReportPhotoService(ReportPhotoHttp reportPhotoHttp)
        {
            _reportPhotoHttp = reportPhotoHttp;
        }

        public async Task<List<ReportPhoto>> GetReportPhotos(string token)
        {
            ReportPhotos = await _reportPhotoHttp.GetReportPhotos(token);

            return ReportPhotos;
        }

        public async Task<ReportPhoto> GetReportPhoto(int ID, string token)
        {
            ReportPhoto reportPhoto = await _reportPhotoHttp.GetReportPhoto(ID, token);

            return reportPhoto;
        }

        public async Task<ReportPhoto> CreateReportPhoto(ReportPhoto reportPhoto, string token)
        {
            reportPhoto = await _reportPhotoHttp.CreateReportPhoto(reportPhoto, token);

            return reportPhoto;
        }

        public async Task<ReportPhoto> UpdateReportPhoto(ReportPhoto reportPhoto, string token)
        {
            reportPhoto = await _reportPhotoHttp.UpdateReportPhoto(reportPhoto, token);

            return reportPhoto;
        }
    }
}
