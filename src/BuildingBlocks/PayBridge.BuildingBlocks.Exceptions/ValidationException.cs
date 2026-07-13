using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Exceptions
{
    public class ValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get;}
        public ValidationException(IReadOnlyDictionary<string, string[]> error) {
            Errors = error;
        }
    }
}
