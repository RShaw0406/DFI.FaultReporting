using DFI.FaultReporting.Services.Interfaces.Files;
using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Services.Files
{
    public class WordFileDetectorService : IFileDetectorService
    {
        public bool IsSafe(byte[] documentBytes)
        {
            bool isSafe = false;

            try
            {
                var ms = new MemoryStream(documentBytes);

                using (var doc = WordprocessingDocument.Open(ms, true))
                {
                    int objectCount = 0;
                    foreach (var part in doc.Parts)
                    {
                        var testForEmbedding = part.OpenXmlPart.GetPartsOfType<EmbeddedPackagePart>();
                        var objectFound = testForEmbedding.Any();

                        if (objectFound)
                            objectCount++;
                    }

                    isSafe = objectCount == 0;
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
