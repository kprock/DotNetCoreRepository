using DotNetCoreRepository.DAL;
using DotNetCoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    public class FBAOrderLogRepository: RepositoryBase<FBAOrderLog>, IFBAOrderLogRepository
    {
        public FBAOrderLogRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public void LogEvent(string amazonOrderId, string ex, string eventType, string source, string action, string contentRootName, string user)
        {
            var log = new FBAOrderLog
            {
                EventDate = DateTime.Now,
                EventDescription = ex,
                EventType = eventType,
                OrderNo = amazonOrderId,
                ContentRootName = contentRootName,
                Source = source,
                Action = action,
                UserName = user
            };

            Insert(log);
        }
    }
}
