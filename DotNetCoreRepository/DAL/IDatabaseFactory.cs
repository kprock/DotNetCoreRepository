using System;
using DotNetCoreRepository.Data;

namespace DotNetCoreRepository.DAL
{
    public interface IDatabaseFactory : IDisposable
    {
        ApplicationDbContext Get();
    }
}
