using System;
using System.Xml;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreRepository.Models
{
    public class AmazonOrder
    {
        [Key]
        public int OrderId { get; set; }
        public string AmazonOrderId { get; set; }
        public string SellerOrderId { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public string OrderStatus { get; set; }
        public string FulfillmentChannel { get; set; }
        public string SalesChannel { get; set; }
        public string OrderChannel { get; set; }
        public string ShipServiceLevel { get; set; }
        
        // address
        public string AddressName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string District { get; set; }
        public string StateOrRegion { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string Phone { get; set; }
        public string AddressType { get; set; }

        public decimal OrderTotal { get; set; }
        public decimal? NumberOfItemsShipped { get; set; }
        public decimal? NumberOfItemsUnshipped { get; set; }
        public string PaymentMethod { get; set; }
        public string MarketplaceId { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerName { get; set; }
        public string BuyerCounty { get; set; }
        public string ShipmentServiceLevelCategory { get; set; }
        public string OrderType { get; set; }
        public DateTime? EarliestShipDate { get; set; }
        public DateTime? LatestShipDate { get; set; }
        public DateTime? EarliestDeliveryDate { get; set; }
        public DateTime? LatestDeliveryDate { get; set; }
        public bool? IsBusinessOrder { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public bool? IsPrime { get; set; }
        public bool? IsPremiumOrder { get; set; }
        public string ReplacedOrderId { get; set; }
        public bool? IsReplacementOrder { get; set; }

        public DateTime? ProcessDate { get; set; }

        // VG
        public int? ProcessingGroupId { get; set; } // nullable. To be auto-assigned.
        public int? ProcessingTypeId { get; set; } // 1. SW: Send license, 2. HW: Send tracking, 3. HW/SW: Send tracking/license
        public int ProcessingStatusId { get; set; } // 1. New, 2. Imported (BP/SO/Pmt), 3. Processed (PO sent), 4. Ready to fulfill, 5. Fulfilled
        public bool FollowUp { get; set; } // checkbox. if follow up, keep on fulfill screen regardless of ProcessingStatusId.
        public string TrackingNumbers { get; set; } // textbox

        // SAP
        public string CardCode { get; set; }
        public string SalesOrderNo { get; set; }
        public string IncomingPaymentNo { get; set; }
        public DateTime InsertDate { get; set; }

        // Nav property to import status
        public virtual ProcessingStatus ProcessingStatus { get; set; }

        // Nav property to child AmazonOrderItem
        public virtual List<AmazonOrderItem> OrderItems { get; set; }
    }
}
