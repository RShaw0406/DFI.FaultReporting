using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Common.Exceptions
{
    public class CustomHttpException : ExceptionBase
    {
        public HttpStatusCode ResponseStatus { get; set; }

        public CustomHttpException() { }

        public CustomHttpException(string message) : base(message) { }

        public CustomHttpException(string message, Exception innerException) : base(message, innerException) { }
    }
}
