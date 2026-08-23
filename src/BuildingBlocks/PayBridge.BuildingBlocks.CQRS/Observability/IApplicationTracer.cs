using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.CQRS.Observability
{
    public interface IApplicationTracer
    {
        Task<T> CaptureSpanAsync<T>(
        string name,
        string type,
        Func<Task<T>> operation);
    }
}
