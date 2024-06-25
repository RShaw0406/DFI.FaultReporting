using DFI.FaultReporting.Services.Interfaces.Files;
using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class ImageFileDetectorService : IFileDetectorService
    {
        public bool IsSafe(byte[] documentBytes)
        {
            bool isSafe = false;
            try
            {

                using (var ms = new MemoryStream(documentBytes))
                {
                    Image initialImage = Image.FromStream(ms);
                    Image reSizedImage;
                    Image sanitizedImage;

                    Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
                    reSizedImage = initialImage.GetThumbnailImage(initialImage.Width - 1, initialImage.Height - 1, myCallback, IntPtr.Zero);
                    sanitizedImage = initialImage.GetThumbnailImage(reSizedImage.Width + 1, reSizedImage.Height + 1, myCallback, IntPtr.Zero);

                    isSafe = true;
                }
            }
            catch (Exception)
            {
                isSafe = false;
            }

            return isSafe;
        }

        public bool ThumbnailCallback()
        {
            return false;
        }
    }
}
