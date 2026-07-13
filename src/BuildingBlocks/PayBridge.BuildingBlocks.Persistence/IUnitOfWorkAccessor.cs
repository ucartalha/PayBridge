using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayBridge.BuildingBlocks.Persistence
{
    public interface IUnitOfWorkAccessor
    {
        bool CanHandle(Type requestType);
        IUnitOfWork UnitOfWork { get; }
    }
}
