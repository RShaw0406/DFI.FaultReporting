using DFI.FaultReporting.Services.Interfaces.Files;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeDetective;

namespace DFI.FaultReporting.Services.Files
{
    public class MimeSnifferService : IMimeSnifferService
    {

        private readonly List<string> WordMimeTypes = new List<string> {
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        };

        private readonly List<string> ImageMimeTypes = new List<string> {
            "image/jpeg",
            "image/png",
        };

        public async Task<bool> IsPdf(byte[] documentBytes)
        {
            return SniffMimeType(documentBytes).Result.Equals("application/pdf");
        }

        public async Task<bool> IsWord(byte[] documentBytes)
        {
            var sniffedMimeType = SniffMimeType(documentBytes);
            return WordMimeTypes.Contains(sniffedMimeType.Result);
        }

        public async Task<bool> IsImage(byte[] documentBytes)
        {
            var sniffedMimeType = SniffMimeType(documentBytes);
            return ImageMimeTypes.Contains(sniffedMimeType.Result);
        }

        public async Task<string> SniffMimeType(byte[] documentBytes)
        {
            var fileType = documentBytes.GetFileType();

            if (fileType != null)
            {
                var result = documentBytes.GetFileType().Mime;
                return documentBytes.GetFileType().Mime;
            }

            return "undetermined";
        }

        public async Task<bool> VerifyMimeType(string fileName, byte[] documentBytes)
        {
            string sniffedMime = await SniffMimeType(documentBytes);
            string fileNameMime = MimeTypes.GetMimeType(fileName);
            return sniffedMime.Equals(fileNameMime, StringComparison.OrdinalIgnoreCase);
        }
    }
}
