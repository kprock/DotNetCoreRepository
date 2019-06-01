using System;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreRepository.Models
{
    public class AmazonOrderItem
    {
        [Key]
        public int ItemId { get; set; }
        public string ASIN { get; set; }
        public string SellerSKU { get; set; }
        public string VendorSKU { get; set; }
        public string OrderItemId { get; set; }
        public string Title { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal QuantityShipped { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal ItemTax { get; set; }
        public decimal ShippingTax { get; set; }
        public DateTime InsertDate { get; set; }

        // FK to AmazonOrder
        public int OrderId { get; set; }
        public AmazonOrder AmazonOrder { get; set; }
    }
}
