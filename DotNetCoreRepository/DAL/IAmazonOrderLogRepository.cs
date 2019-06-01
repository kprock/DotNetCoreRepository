using DotNetCoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCoreRepository.DAL
{
    public interface IAmazonOrderLogRepository : IRepository<AmazonOrderLog>
    {
        void LogEvent(string amazonOrderId, string eventDesc, string eventType, string contentRootName, string source, string action, string userName);
    }
}
