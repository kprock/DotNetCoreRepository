using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreRepository.Data;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreRepository.DAL
{
    public class DatabaseFactory : Disposable, IDatabaseFactory
    {
        private ApplicationDbContext _database;

        public ApplicationDbContext Get()
        {
            return _database ?? (_database = new ApplicationDbContextFactory().Create());
        }

        protected override void DisposeCore()
        {
            if (_database != null)
            {
                _database.Dispose();
            }
        }
    }
}
