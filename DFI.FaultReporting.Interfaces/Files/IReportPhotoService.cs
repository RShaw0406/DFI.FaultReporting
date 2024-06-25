using DFI.FaultReporting.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Interfaces.Files
{
    public interface IReportPhotoService
    {
        Task<List<ReportPhoto>> GetReportPhotos(string token);

        Task<ReportPhoto> GetReportPhoto(int ID, string token);

        Task<ReportPhoto> CreateReportPhoto(ReportPhoto reportPhoto, string token);

        Task<ReportPhoto> UpdateReportPhoto(ReportPhoto reportPhoto, string token);

        Task<int> DeleteReportPhoto(int ID, string token);
    }
}
