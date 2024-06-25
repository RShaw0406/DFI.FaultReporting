using DFI.FaultReporting.Services.Interfaces.Files;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class FileValidationService : IFileValidationService
    {
        private readonly IMimeSnifferService _mimeSnifferService;

        public FileValidationService(IMimeSnifferService mimeSnifferService)
        {
            _mimeSnifferService = mimeSnifferService;
        }

        public async Task<bool> ValidateDocument(IFormFile document)
        {
            var result = true;

            //check user supplied filename
            if (Path.GetFileNameWithoutExtension(document.FileName).IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            //check user supplied filename is present
            if (Path.GetFileNameWithoutExtension(document.FileName).Length == 0)
            {
                return false;
            }

            byte[] documentBytes = null;
            using (var ms = new MemoryStream())
            {
                document.CopyTo(ms);
                documentBytes = ms.ToArray();
            }

            if (CanVerifyMimeType())
            {
                result = result && await VerifyMimeType(document.FileName, documentBytes);
            }

            if (CanCheckForDoubleFileExtension())
            {
                result = result && CheckForDoubleFileExtension(document.FileName);
            }

            if (result)
            {
                if (await IsPdfAsync(documentBytes))
                {
                    PDFFileDetectorService detector = new PDFFileDetectorService();
                    result = result && detector.IsSafe(documentBytes);
                }
                else if (await IsWordAsync(documentBytes))
                {
                    WordFileDetectorService detector = new WordFileDetectorService();
                    result = result && detector.IsSafe(documentBytes);
                }
                else if (await IsImageAsync(documentBytes))
                {
                    ImageFileDetectorService detector = new ImageFileDetectorService();
                    result = result && detector.IsSafe(documentBytes);
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        public async Task<bool> SanitizeImage(IFormFile document)
        {
            var result = true;

            //check user supplied filename
            if (Path.GetFileNameWithoutExtension(document.FileName).IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                return false;
            }

            //check user supplied filename is present
            if (Path.GetFileNameWithoutExtension(document.FileName).Length == 0)
            {
                return false;
            }

            byte[] documentBytes = null;
            using (var ms = new MemoryStream())
            {
                document.CopyTo(ms);
                documentBytes = ms.ToArray();
            }


            if (CanVerifyMimeType())
            {
                result = result && await VerifyMimeType(document.FileName, documentBytes);
            }

            if (CanCheckForDoubleFileExtension())
            {
                result = result && CheckForDoubleFileExtension(document.FileName);
            }

            if (result)
            {
                if (await IsImageAsync(documentBytes))
                {
                    ImageFileDetectorService detector = new ImageFileDetectorService();
                    result = result && detector.IsSafe(documentBytes);
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        protected bool CanVerifyMimeType()
        {
            return true;
        }

        protected async Task<bool> VerifyMimeType(string fileName, byte[] documentBytes)
        {
            return await _mimeSnifferService.VerifyMimeType(fileName, documentBytes);
        }

        protected async Task<bool> IsPdfAsync(byte[] documentBytes)
        {
            return await _mimeSnifferService.IsPdf(documentBytes);
        }

        protected async Task<bool> IsWordAsync(byte[] documentBytes)
        {
            return await _mimeSnifferService.IsWord(documentBytes);
        }

        protected async Task<bool> IsImageAsync(byte[] documentBytes)
        {
            return await _mimeSnifferService.IsImage(documentBytes);
        }

        private bool CanCheckForDoubleFileExtension()
        {
            return true;
        }

        private bool CheckForDoubleFileExtension(string filename)
        {
            int docx = Regex.Matches(filename, ".docx").Count;
            int pdf = Regex.Matches(filename, ".pdf").Count;
            int doc = Regex.Matches(filename, ".doc").Count;

            if (docx > 1 || pdf > 1 || doc > 1)
                return false;
            else
                return true;
        }
    }
}
