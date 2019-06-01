using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class OrderItem
    {
        public TaxCollection TaxCollection { get; set; }
        public BuyerCustomizedInfoDetail BuyerCustomizedInfo { get; set; }
        public string ASIN { get; set; }
        public string SellerSKU { get; set; }
        public string OrderItemId { get; set; }
        public string Title { get; set; }
        public decimal QuantityOrdered { get; set; }
        public decimal QuantityShipped { get; set; }
        public ProductInfoDetail ProductInfo { get; set; }
        public PointsGrantedDetail PointsGranted { get; set; }
        public Money ItemPrice { get; set; }
        public Money ShippingPrice { get; set; }
        public Money GiftWrapPrice { get; set; }
        public Money ItemTax { get; set; }
        public Money ShippingTax { get; set; }
        public string ConditionNote { get; set; }
        public string PriceDesignation { get; set; }
        public string ScheduledDeliveryEndDate { get; set; }
        public string ScheduledDeliveryStartDate { get; set; }
        public string ConditionSubtypeId { get; set; }
        public string ConditionId { get; set; }
        public Money GiftWrapTax { get; set; }
        public InvoiceData InvoiceData { get; set; }
        public string GiftMessageText { get; set; }
        public Money CODFeeDiscount { get; set; }
        public Money CODFee { get; set; }
        public List<string> PromotionIds { get; set; }
        public Money PromotionDiscount { get; set; }
        public Money ShippingDiscount { get; set; }
        public string GiftWrapLevel { get; set; }
    }
}
