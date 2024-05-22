using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFI.FaultReporting.Common.Exceptions
{
    public class ExceptionBase : Exception
    {
        public string ExceptionFunction { get; set; }
        public string ExceptionClass { get; set; }

        protected ExceptionBase() { }
        protected ExceptionBase(string message) : base(message) { }

        protected ExceptionBase(string message, Exception innerException) : base(message, innerException) { }
    }
}
