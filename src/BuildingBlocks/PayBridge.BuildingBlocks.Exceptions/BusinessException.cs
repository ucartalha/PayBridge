using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Exceptions
{
    public sealed class BusinessException: Exception
    {
        public int ErrorCode{ get; set; }
        public BusinessException(int errorCode) : base($"Business rule violation occurred. ErrorCode: {errorCode}") 
        {
            ErrorCode = errorCode;        
        }
        public BusinessException(int errorCode, Exception innerException)
       : base($"Business rule violation occurred. ErrorCode: {errorCode}", innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
