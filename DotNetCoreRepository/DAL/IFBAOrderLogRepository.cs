using DotNetCoreRepository.DAL;
using DotNetCoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    public interface IFBAOrderLogRepository: IRepository<FBAOrderLog>
    {
        void LogEvent(string amazonOrderId, string ex, string level, string source, string action, string contentRootName, string user);
    }
}
