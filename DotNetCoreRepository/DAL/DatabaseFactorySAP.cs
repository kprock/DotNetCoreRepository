using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreRepository.Data;

namespace DotNetCoreRepository.DAL
{
    public class DatabaseFactorySAP : Disposable, IDatabaseFactorySAP
    {
        private SAPDbContext _database;

        public SAPDbContext Get()
        {
            return _database ?? (_database = new SAPDbContextFactory().Create());
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
