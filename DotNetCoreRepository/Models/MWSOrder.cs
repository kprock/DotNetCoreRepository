using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class MWSOrder
    {
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; }
    }
}
