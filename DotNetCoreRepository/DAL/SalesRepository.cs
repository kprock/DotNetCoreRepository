using DotNetCoreRepository.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    public class SalesRepository : RepositoryBase<SalesHistory>, ISalesRepository
    {
        public SalesRepository(IDatabaseFactory databaseFactory) : base(databaseFactory){}

        public List<SalesHistory> GetSalesHistory(DateTime startDate, DateTime endDate)
        {
            return Database.SalesHistory.FromSql("usp_SalesHistory @p0, @p1", startDate.ToString(), endDate.ToString()).ToList();
        }
    }
}
