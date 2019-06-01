using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class PaymentExecutionDetailItem
    {
        public Money Payment { get; set; }
        public string PaymentMethod { get; set; }
    }
}
