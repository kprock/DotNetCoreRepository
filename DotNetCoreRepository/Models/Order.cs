using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class Order
    {
        public string BuyerCounty { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string MarketplaceId { get; set; }
        public List<string> PaymentMethodDetails { get; set; }
        public string PaymentMethod { get; set; }
        public List<PaymentExecutionDetailItem> PaymentExecutionDetail { get; set; }
        public decimal NumberOfItemsUnshipped { get; set; }
        public decimal NumberOfItemsShipped { get; set; }
        public BuyerTaxInfo BuyerTaxInfo { get; set; }
        public Money OrderTotal { get; set; }
        public string ShipServiceLevel { get; set; }
        public string OrderChannel { get; set; }
        public string ReplacedOrderId { get; set; }
        public string SalesChannel { get; set; }
        public string FulfillmentChannel { get; set; }
        public string OrderStatus { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string SellerOrderId { get; set; }
        public string AmazonOrderId { get; set; }
        public Address ShippingAddress { get; set; }
        public string ShipmentServiceLevelCategory { get; set; }
        public bool IsReplacementOrder { get; set; }
        public bool ShippedByAmazonTFM { get; set; }
        public bool IsPremiumOrder { get; set; }
        public bool IsPrime { get; set; }
        public string TFMShipmentStatus { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public bool IsBusinessOrder { get; set; }
        public DateTime LatestDeliveryDate { get; set; }
        public string CbaDisplayableShippingLabel { get; set; }
        public DateTime EarliestDeliveryDate { get; set; }
        public string OrderType { get; set; }
        public DateTime LatestShipDate { get; set; }
        public DateTime EarliestShipDate { get; set; }
    }
}
