using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class InvoiceData
    {
        public string InvoiceRequirement { get; set; }
        public string BuyerSelectedInvoiceCategory { get; set; }
        public string InvoiceTitle { get; set; }
        public string InvoiceInformation { get; set; }
    }
}
