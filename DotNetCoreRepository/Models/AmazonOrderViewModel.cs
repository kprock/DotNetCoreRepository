using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    [NotMapped]
    public class AmazonOrderViewModel
    {
        [Key]
        public int OrderId { get; set; }

        /// <summary>
        /// VG processing team
        /// </summary>
        public string VGGrp { get; set; }

        /// <summary>
        /// Concatenation of AmazonOrderID and BuyerName
        /// </summary>
        public string IdCust { get; set; }

        public string AmazonOrderId { get; set; }

        public string ProcessingStatus { get; set; }

        public string ProcessingType { get; set; }

        public string Notes { get; set; }

        public string SKU { get; set; }

        public string SO { get; set; }

        public string PO { get; set; }

        public string Vendor { get; set; }

        /// <summary>
        /// Original SQL DateTime
        /// </summary>
        public DateTime PurchaseDateSQL { get; set; }

        /// <summary>
        /// DateTime converted to Javascript datestamp
        /// </summary>
        public long PurchaseDate { get; set; }

        /// <summary>
        /// Original SQL DateTime
        /// </summary>
        public DateTime LatestShipDateSQL { get; set; }

        /// <summary>
        /// DateTime converted to Javascript datestamp
        /// </summary>
        public long LatestShipDate { get; set; }

        public DateTime LatestDeliveryDate { get; set; }

        /// <summary>
        /// Nullable. Order process date. I.E., PO created.
        /// </summary>
        public DateTime? ProcessDate { get; set; }

        public string State { get; set; }

        public decimal OrderTotal { get; set; }

        public string OrderTotalFormatted { get; set; }

        public string Prime { get; set; }
        public bool IsPrime { get; set; }

        public string Premium { get; set; }
        public bool IsPremiumOrder { get; set; }

        public string Tracking { get; set; }
    }
}
