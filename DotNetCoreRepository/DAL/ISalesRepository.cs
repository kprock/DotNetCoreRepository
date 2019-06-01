using DotNetCoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    public interface ISalesRepository : IRepository<SalesHistory>
    {
        List<SalesHistory> GetSalesHistory(DateTime startDate, DateTime endDate);
    }
}
