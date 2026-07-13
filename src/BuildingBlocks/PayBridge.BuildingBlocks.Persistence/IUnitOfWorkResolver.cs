using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Persistence
{
    public interface IUnitOfWorkResolver
    {
        IUnitOfWork? Resolve(Type requestType);
    }
}
