using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class SalesHistory
    {
        [Key]
        public string SlpName { get; set; }
        public int Invs { get; set; }
        public int CMs { get; set; }
        public decimal LineRevenue { get; set; }
        public decimal ShippingRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal LineCost { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Rebate { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
    }
}
