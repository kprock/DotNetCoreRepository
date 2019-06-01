using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class Address
    {
        public string AddressType { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine1 { get; set; }
        public string Name { get; set; }
        public string District { get; set; }
        public string StateOrRegion { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string County { get; set; }
    }
}
