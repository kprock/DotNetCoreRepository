using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class BuyerTaxInfo
    {
        public string CompanyLegalName { get; set; }
        public string TaxingRegion { get; set; }
        public List<TaxClassification> TaxClassifications { get; set; }
    }
}
