using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class BPAddress
    {
        [Key]
        public int AddressID { get; set; }

        [MaxLength(50)]
        public string AddressName { get; set; }

        [MaxLength(100)]
        public string Street { get; set; }

        [MaxLength(100)]
        public string Block { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        // Required for Canada, USA and Australia
        [MaxLength(3)]
        public string StateCode { get; set; }

        [MaxLength(20)]
        public string ZipCode { get; set; }

        [MaxLength(3)]
        public string CountryCode { get; set; }

        [MaxLength(25)]
        public string PhoneNumber { get; set; }

        public bool MarketingOptOut { get; set; }
     }
}
