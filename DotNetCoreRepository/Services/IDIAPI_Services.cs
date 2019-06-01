using DotNetCoreRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Services
{
    public interface IDIAPI_Services
    {
        string CreateBusinessPartner(AmazonOrder o);

        Dictionary<string, string> CreateSalesOrder(string cardCode, AmazonOrder o);

        string CreatePayment(string cardCode, string salesOrderNo, AmazonOrder order);
    }
}
