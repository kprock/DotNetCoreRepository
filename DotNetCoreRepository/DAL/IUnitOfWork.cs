using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    public interface IUnitOfWork
    {
        void Commit();
    }
}
