using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreRepository.Data;

namespace DotNetCoreRepository.DAL
{
    public abstract class RepositoryBaseSAP
    {
        private SAPDbContext _database;

        public RepositoryBaseSAP(IDatabaseFactorySAP databaseFactory)
        {
            DatabaseFactorySAP = databaseFactory;
        }

        protected IDatabaseFactorySAP DatabaseFactorySAP
        {
            get;
            private set;
        }

        protected SAPDbContext DatabaseSAP
        {
            get { return _database ?? (_database = DatabaseFactorySAP.Get()); }
        }
    }
}
