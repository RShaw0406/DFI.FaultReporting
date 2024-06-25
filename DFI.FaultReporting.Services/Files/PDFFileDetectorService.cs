using DFI.FaultReporting.Services.Interfaces.Files;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class PDFFileDetectorService : IFileDetectorService
    {
        public bool IsSafe(byte[] documentBytes)
        {
            bool isSafe = false;

            try
            {
                using (var reader = new PdfReader(documentBytes))
                {

                    RandomAccessFileOrArray r = new RandomAccessFileOrArray(documentBytes);
                    var jsCode = reader.GetJavaScript(r);

                    if (jsCode == null)
                    {
                        PdfDictionary root = reader.Catalog;
                        PdfDictionary names = root.GetAsDict(PdfName.NAMES);

                        PdfArray namesArray = null;
                        if (names != null)
                        {
                            PdfDictionary embeddedFiles = names.GetAsDict(PdfName.EMBEDDEDFILES);
                            if (embeddedFiles != null)
                            {
                                namesArray = embeddedFiles.GetAsArray(PdfName.NAMES);
                            }
                        }

                        isSafe = ((namesArray == null) || namesArray.Length == 0);
                    }
                }
            }
            catch (Exception)
            {
                isSafe = false;
            }

            return isSafe;
        }
    }
}
