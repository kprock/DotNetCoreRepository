using DotNetCoreRepository.Models;
using DotNetCoreRepository.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetCoreRepository.DAL
{
    public class AmazonOrderLogRepository : RepositoryBase<AmazonOrderLog>, IAmazonOrderLogRepository
    {
        public AmazonOrderLogRepository(
            IDatabaseFactory databaseFactory
            ) : base(databaseFactory)
        {
        }

        public void LogEvent(string amazonOrderId, string eventDesc, string eventType, string contentRootName, string source, string action, string userName)
        {
            var log = new AmazonOrderLog
            {
                EventDate = DateTime.Now,
                EventDescription = eventDesc,
                EventType = eventType,
                OrderNo = amazonOrderId,
                ContentRootName = contentRootName,
                Source = source,
                Action = action,
                UserName = userName.IsNullOrWhitespace() ? "GLaDOS" : userName
            };

            Insert(log);
        }
    }
}
