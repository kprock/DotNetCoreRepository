using DotNetCoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Services
{
    public interface IDIAPI_Services_FBA
    {
        string CreateBusinessPartner(MWSOrder o);

        Dictionary<string, string> CreateInvoice(string cardCode, MWSOrder o);

        string CreatePayment(string cardCode, string invDocEntry, MWSOrder order);
    }
}