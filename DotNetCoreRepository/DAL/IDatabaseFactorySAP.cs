using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreRepository.Data;

namespace DotNetCoreRepository.DAL
{
    public interface IDatabaseFactorySAP : IDisposable
    {
        SAPDbContext Get();
    }
}
