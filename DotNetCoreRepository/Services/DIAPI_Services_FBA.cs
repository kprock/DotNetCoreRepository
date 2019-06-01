using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreRepository.DAL;
using DotNetCoreRepository.Extensions;
using DotNetCoreRepository.Models;
using SAPbobsCOM;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Diagnostics;
using DotNetCoreRepository.Enums;

namespace DotNetCoreRepository.Services
{
    static class DIAPI_Objects_FBA
    {
        public static Company Company;
        public static BusinessPartners BusinessPartner;
        public static Documents Invoice;
        public static Payments Payment;

        /// <summary>
        /// Connect to SAP Company and Db
        /// </summary>
        /// <returns>Log object.</returns>
        public static FBAOrderLog Connect()
        {
            if (Company != null)
            {
                if (Company.Connected)
                {
                    var l = new FBAOrderLog
                    {
                        EventDescription = "success"
                    };
                    return l;
                }
            }

            try
            {
                Company = new Company
                {
                    DbServerType = BoDataServerTypes.dst_MSSQL2012,
                    language = BoSuppLangs.ln_English,
                    UseTrusted = false,
                    Server = Constants.SapDiapiSettings.B1_SERVER,
                    LicenseServer = Constants.SapDiapiSettings.B1_LICENSE_SERVER,
                    DbUserName = Constants.SapDiapiSettings.B1_DB_USER_NAME,
                    DbPassword = Constants.SapDiapiSettings.B1_DB_PASSWORD,
                    CompanyDB = Constants.SapDiapiSettings.B1_COMPANY_DB, // change to COMPANY_DB when go live
                    UserName = Constants.SapDiapiSettings.B1_USER_NAME,
                    Password = Constants.SapDiapiSettings.B1_PASSWORD
                };

                long RetCode = Company.Connect();

                if (RetCode != 0)
                {
                    Company.GetLastError(out int errCode, out string errMsg);

                    var l = new FBAOrderLog
                    {
                        EventDate = DateTime.Now,
                        EventDescription = errCode + " - " + errMsg,
                        EventType = nameof(LogEventLevel.Error),
                        ContentRootName = "Bast",
                        Source = "DIAPI_Objects_FBA",
                        Action = StackExtensions.GetCurrentMethod(),
                        UserName = "SHODAN"
                    };

                    return l;
                }

                if (Company.Connected)
                {
                    var l = new FBAOrderLog
                    {
                        EventDescription = "success"
                    };

                    return l;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                var l = new FBAOrderLog
                {
                    EventDate = DateTime.Now,
                    EventDescription = ex.GetType().Name + " - " + ex.Message,
                    EventType = nameof(LogEventLevel.Critical),
                    ContentRootName = "Bast",
                    Source = "DIAPI_Objects_FBA",
                    Action = StackExtensions.GetCurrentMethod(),
                    UserName = "GLaDOS"
                };

                return l;
            }
        }

        public static void Disconnect()
        {
            try
            {
                if (Company != null)
                {
                    if (Company.Connected)
                        Company.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Company.GetLastError(out int errCode, out string errMsg);
                Console.Write(ex.Message);
            }
            finally
            {
                Company = null;
            }
        }
    }

    public class DIAPI_Services_FBA : IDIAPI_Services_FBA
    {
        private long RetCode;
        private int errCode;
        private string errMsg;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IFBAOrderLogRepository _orderLogRepository;
        private readonly ISAPDataService _sapDataService;

        public DIAPI_Services_FBA(
            IUnitOfWork unitOfWork,
            IFBAOrderLogRepository orderLogRepository,
            ISAPDataService sapDataService
            )
        {
            _unitOfWork = unitOfWork;
            _orderLogRepository = orderLogRepository;
            _sapDataService = sapDataService;
        }

        /// <summary>
        /// Creates a BP.
        /// </summary>
        /// <param name="o">The Amazon order object.</param>
        /// <returns>The BP's CardCode or null on error.</returns>
        public string CreateBusinessPartner(MWSOrder o)
        {
            try
            {
                // If state is longer than two characters, find abbreviation and update Amazon order.
                // If unable to resolve state, throw exception.
                string stateCode = null;
                if (o.Order.ShippingAddress.StateOrRegion?.Length > 2) // potentially full name of state
                {
                    var s = _sapDataService.GetStateCode(o.Order.ShippingAddress.StateOrRegion);
                    stateCode = s.StateCode;
                    if (!s.StateCode.IsNullOrWhitespace()) // abbreviation found: Set Amazon-order stateCode to abbreviation
                    {
                        o.Order.ShippingAddress.StateOrRegion = stateCode;
                    }
                    else // unable to find stateCode. Allow BP creation to continue (allowing for correction), even though invoice creation will fail.
                    {
                        o.Order.ShippingAddress.StateOrRegion = null;

                        _orderLogRepository.LogEvent(
                            o.Order.AmazonOrderId,
                            "Shipping state could not be resolved. Please update manually, as invoice creation will fail.",
                            nameof(LogEventLevel.Error),
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                            "Bast",
                            "HAL 9000");
                    }
                }

                // Get Matching BPs from SAP for user's email.
                var bp = _sapDataService.GetCardCodeForEmail(o.Order.BuyerEmail?.Trim());

                if (bp.Count > 0) // Matching BP found. Grab first CardCode in list (most recent Sales Order date) and return.
                {
                    if (bp.Count > 1) // At least one dupe BP. Deactivate.
                    {
                        int i = _sapDataService.DeactivateRedundantBPs(o.Order.BuyerEmail?.Trim(), bp[0].CardCode);
                    }

                    // Add both shipping and billing addresses using the Amazon order's Shipping address
                    var addr = new BPAddress
                    {
                        AddressName = o.Order.ShippingAddress.Name,
                        Street = o.Order.ShippingAddress.AddressLine1,
                        Block = o.Order.ShippingAddress.AddressLine2,
                        City = o.Order.ShippingAddress.City,
                        StateCode = o.Order.ShippingAddress.StateOrRegion,
                        ZipCode = o.Order.ShippingAddress.PostalCode,
                        CountryCode = o.Order.ShippingAddress.CountryCode
                    };

                    _sapDataService.AddAddress(bp[0].CardCode, 1, addr); // shipping
                    _sapDataService.AddAddress(bp[0].CardCode, 2, addr); // billing

                    _orderLogRepository.LogEvent(
                        o.Order.AmazonOrderId,
                        "BP " + bp[0].CardCode + " already exists. Billing and shipping addresses updated.",
                        nameof(LogEventLevel.Information),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "Bast",
                        "HAL 9000");

                    _unitOfWork.Commit();

                    return bp[0].CardCode;
                }

                // create  new Business Partner
                DIAPI_Objects_FBA.BusinessPartner = (BusinessPartners)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
                DIAPI_Objects_FBA.BusinessPartner.CardName = o.Order.BuyerName;
                DIAPI_Objects_FBA.BusinessPartner.CardType = BoCardTypes.cCustomer;
                DIAPI_Objects_FBA.BusinessPartner.Phone1 = o.Order.ShippingAddress.Phone;
                DIAPI_Objects_FBA.BusinessPartner.EmailAddress = !o.Order.BuyerEmail.IsNullOrWhitespace() ? o.Order.BuyerEmail : "missing.email@fakedomain.com";
                DIAPI_Objects_FBA.BusinessPartner.ContactPerson = o.Order.BuyerName;
                DIAPI_Objects_FBA.BusinessPartner.Notes = "Auto-created by AmazonFBAOrder";
                DIAPI_Objects_FBA.BusinessPartner.PayTermsGrpCode = 5; // Credit Card
                DIAPI_Objects_FBA.BusinessPartner.Valid = BoYesNoEnum.tYES;

                DIAPI_Objects_FBA.BusinessPartner.Addresses.Add();

                // Shipping
                DIAPI_Objects_FBA.BusinessPartner.Addresses.SetCurrentLine(0);
                DIAPI_Objects_FBA.BusinessPartner.Addresses.AddressType = BoAddressType.bo_ShipTo;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.AddressName = o.Order.ShippingAddress.Name;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.Street = o.Order.ShippingAddress.AddressLine1;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.Block = o.Order.ShippingAddress.AddressLine2;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.City = o.Order.ShippingAddress.City;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.State = o.Order.ShippingAddress.StateOrRegion;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.ZipCode = o.Order.ShippingAddress.PostalCode;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.Country = o.Order.ShippingAddress.CountryCode;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.UserFields.Fields.Item("U_MarketingOptOut").Value = 0;

                // Billing
                DIAPI_Objects_FBA.BusinessPartner.Addresses.SetCurrentLine(1);
                DIAPI_Objects_FBA.BusinessPartner.Addresses.AddressType = BoAddressType.bo_BillTo;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.AddressName = o.Order.ShippingAddress.Name;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.Street = o.Order.ShippingAddress.AddressLine1;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.Block = o.Order.ShippingAddress.AddressLine2;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.City = o.Order.ShippingAddress.City;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.State = o.Order.ShippingAddress.StateOrRegion;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.ZipCode = o.Order.ShippingAddress.PostalCode;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.Country = o.Order.ShippingAddress.CountryCode;
                DIAPI_Objects_FBA.BusinessPartner.Addresses.UserFields.Fields.Item("U_MarketingOptOut").Value = 0;

                DIAPI_Objects_FBA.BusinessPartner.CardCode = GetNextID("BusinessPartner"); ;

                // contact info
                DIAPI_Objects_FBA.BusinessPartner.ContactEmployees.Add();
                DIAPI_Objects_FBA.BusinessPartner.ContactEmployees.SetCurrentLine(0);
                DIAPI_Objects_FBA.BusinessPartner.ContactEmployees.Name = o.Order.ShippingAddress.Name;
                DIAPI_Objects_FBA.BusinessPartner.ContactEmployees.Address = o.Order.ShippingAddress.AddressLine1 + ", " + o.Order.ShippingAddress.City + ", " + o.Order.ShippingAddress.StateOrRegion + " " + o.Order.ShippingAddress.PostalCode + " " + o.Order.ShippingAddress.CountryCode;
                DIAPI_Objects_FBA.BusinessPartner.ContactEmployees.E_Mail = o.Order.BuyerEmail;
                DIAPI_Objects_FBA.BusinessPartner.ContactEmployees.Phone1 = o.Order.ShippingAddress.Phone;

                RetCode = DIAPI_Objects_FBA.BusinessPartner.Add();

                // Save CardCode to log and update OrderHeader and UserAccount
                string CardCode = DIAPI_Objects_FBA.BusinessPartner.CardCode;

                if (RetCode != 0)
                {
                    DIAPI_Objects_FBA.Company.GetLastError(out errCode, out errMsg);

                    _orderLogRepository.LogEvent(
                         o.Order.AmazonOrderId,
                         errCode + " - " + errMsg + " - BP was not created.",
                         nameof(LogEventLevel.Critical),
                         "DIAPI_Services",
                         StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");
                }
                else
                {
                    _orderLogRepository.LogEvent(
                         o.Order.AmazonOrderId,
                         "BP " + CardCode + " was created.",
                         nameof(LogEventLevel.Information),
                         "DIAPI_Services",
                         StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");
                }

                _unitOfWork.Commit();
                return CardCode;
            }
            catch (Exception ex)
            {
                _orderLogRepository.LogEvent(
                     o.Order.AmazonOrderId,
                     ex.GetType().Name + " - " + ex.Message,
                     nameof(LogEventLevel.Critical),
                     "DIAPI_Services",
                     StackExtensions.GetCurrentMethod(),
                    "Bast",
                    "HAL 9000");

                _unitOfWork.Commit();

                return null;
            }
        }

        /// <summary>
        /// Create SAP Invoice
        /// </summary>
        /// <param name="cardCode">The CardCode for the BP.</param>
        /// <param name="o">The Amazon order object</param>
        /// <returns>Dictionary with DocEntry and DocNum of the invoice.</returns>
        public Dictionary<string, string> CreateInvoice(string cardCode, MWSOrder o)
        {
            double totalItemPrice = 0, totalItemTax = 0, totalShipCost = 0, totalShipTax = 0;
            string DocEntry = null, q;
            int DocNum = 0;
            Dictionary<string, string> InvoiceNums = new Dictionary<string, string>();
            Recordset rs;

            try
            {
                // If invoice exists with AmzOrderId, log and return
                q = "usp_MarketplaceGetDocNumByAmazonOrderID '" + o.Order.AmazonOrderId + "'";
                rs = (Recordset)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(q);
                if (!(rs.EoF)) // Invoice exists
                {
                    _orderLogRepository.LogEvent(
                        o.Order.AmazonOrderId,
                        "Invoice #" + rs.Fields.Item(0).Value.ToString() + " already exists in SAP.",
                        nameof(LogEventLevel.Warning),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "Bast",
                        "HAL 9000");

                    InvoiceNums.Add("DocNum", rs.Fields.Item(0).Value.ToString());
                    InvoiceNums.Add("DocEntry", rs.Fields.Item(1).Value.ToString());

                    // Check if invoice total matches Amazon order total
                    var validInvTotal = Decimal.TryParse(rs.Fields.Item(2).Value.ToString(), out decimal invTotal);
                    var validAmzTotal = Decimal.TryParse(o.Order.OrderTotal.Amount, out decimal amzTotal);

                    if (validInvTotal && validAmzTotal)
                    {
                        if (Decimal.Compare(invTotal, amzTotal) != 0)
                        {
                            bool shipToWA = false;
                            if (o.Order.ShippingAddress.StateOrRegion == "WA" || o.Order.ShippingAddress.StateOrRegion == "Washington")
                                shipToWA = true;

                            _orderLogRepository.LogEvent(
                                o.Order.AmazonOrderId,
                                (shipToWA ? "Shipped to WA: " : "") + "Invoice total $" + invTotal.ToString() + " doesn't equal Amazon-order total of $" + amzTotal.ToString() + ". Please verify that this isn't an error.",
                                nameof(LogEventLevel.Warning),
                                "DIAPI_Services",
                                StackExtensions.GetCurrentMethod(),
                                 "Bast",
                                 "HAL 9000");
                        }
                    }

                    _unitOfWork.Commit();

                    return InvoiceNums;
                }
                rs = null;

                DIAPI_Objects_FBA.Invoice = (Documents)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.oInvoices);
                DIAPI_Objects_FBA.Invoice.CardCode = cardCode;
                DIAPI_Objects_FBA.Invoice.NumAtCard = o.Order.AmazonOrderId.Trim();
                DIAPI_Objects_FBA.Invoice.HandWritten = BoYesNoEnum.tNO;
                DIAPI_Objects_FBA.Invoice.DocDate = DateTime.Now;
                DIAPI_Objects_FBA.Invoice.DocDueDate = DateTime.Now;
                DIAPI_Objects_FBA.Invoice.DocCurrency = "$";
                DIAPI_Objects_FBA.Invoice.SalesPersonCode = 68; // Amazon slpcode
                DIAPI_Objects_FBA.Invoice.TransportationCode = 13; // Ground

                // Calculate shipping
                foreach (var l in o.OrderItems)
                {
                    var s = l.ShippingPrice == null ? "0" : l.ShippingPrice?.Amount;
                    //var t = l.ShippingTax == null ? "0" : l.ShippingTax?.Amount;
                    if (Double.TryParse(s, out double shipCost))
                    {
                        totalShipCost += shipCost;
                        //if (Double.TryParse(t, out double shipTax))
                        //{
                        //    totalShipTax += shipTax;
                        //}
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid shipping cost for " + l.SellerSKU + " - " + l.Title);
                    }
                }

                DIAPI_Objects_FBA.Invoice.Expenses.ExpenseCode = 1;
                DIAPI_Objects_FBA.Invoice.Expenses.LineTotal = totalShipCost;
                DIAPI_Objects_FBA.Invoice.Expenses.Add();

                bool partFound = false;
                foreach (var line in o.OrderItems)
                {
                    //if quantity shipped is zero, continue
                    if (line.QuantityShipped == 0)
                        continue;

                    // Amazon: get mfr SKU by substring to left of underscore.
                    int index = line.SellerSKU.IndexOf("_");
                    string sku = (index > 0 ? line.SellerSKU.Substring(0, index) : line.SellerSKU);

                    q = "usp_findPartBySKU '" + sku + "'";
                    rs = (Recordset)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(q);
                    if (!(rs.EoF))
                    {
                        DIAPI_Objects_FBA.Invoice.Lines.ItemCode = rs.Fields.Item(0).Value.ToString();
                        DIAPI_Objects_FBA.Invoice.Lines.Quantity = Convert.ToDouble(line.QuantityShipped);
                        DIAPI_Objects_FBA.Invoice.Lines.WarehouseCode = "AM1"; // FBA orders ship from Amazon's warehouse.

                        // Add serial numbers
                        var sn = _sapDataService.GetSerialNumbers(sku, Convert.ToInt32(line.QuantityShipped));

                        // ensure that available serial numbers/license keys exist for SKU, in the required quantity
                        if (sn != null && sn.Count >= line.QuantityShipped)
                        {
                            for (int k = 0; k < Convert.ToInt32(line.QuantityShipped); k++)
                            {
                                DIAPI_Objects_FBA.Invoice.Lines.SerialNumbers.SetCurrentLine(k);
                                DIAPI_Objects_FBA.Invoice.Lines.SerialNumbers.ManufacturerSerialNumber = sn[k].SuppSerial;
                                DIAPI_Objects_FBA.Invoice.Lines.SerialNumbers.Quantity = 1;
                                DIAPI_Objects_FBA.Invoice.Lines.SerialNumbers.Add();
                            }
                        }
                        else // throw exception.
                        {
                            throw new SerialNumberNotFoundException(sku + ": Available serial#/license key was either not found or not enough to match quantity.");
                        }

                        // Set tax
                        if (Double.TryParse(line.ItemTax?.Amount, out double itemTax))
                        {
                            DIAPI_Objects_FBA.Invoice.Lines.TaxTotal = itemTax;

                            if ((string.Equals(o.Order.ShippingAddress.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) || string.Equals(o.Order.ShippingAddress.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase)) && itemTax == 0)
                            {
                                DIAPI_Objects_FBA.Invoice.Lines.TaxCode = "Exempt";

                                _orderLogRepository.LogEvent(
                                   o.Order.AmazonOrderId,
                                   "Adding " + sku + " - Shipping address is CA, but no sales tax was charged.",
                                   nameof(LogEventLevel.Warning),
                                   "DIAPI_Services",
                                   StackExtensions.GetCurrentMethod(),
                                 "Bast",
                                 "HAL 9000");
                            }
                            else if (!string.Equals(o.Order.ShippingAddress.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) && !string.Equals(o.Order.ShippingAddress.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase))
                            {
                                // no tax outside of california
                                DIAPI_Objects_FBA.Invoice.Lines.TaxCode = "Exempt";
                                DIAPI_Objects_FBA.Invoice.Lines.TaxTotal = 0;
                            }
                            else // California
                            {
                                // Set tax
                                DIAPI_Objects_FBA.Invoice.Lines.TaxCode = "CA";
                                DIAPI_Objects_FBA.Invoice.Lines.TaxTotal = itemTax;
                                //totalItemTax += itemTax;
                            }
                        }
                        else // invalid number, set tax to zero
                        {
                            DIAPI_Objects_FBA.Invoice.Lines.TaxTotal = 0;

                            _orderLogRepository.LogEvent(
                                o.Order.AmazonOrderId,
                                "Adding " + sku + ": Tax format invalid. Unable to set sales tax. Please validate tax manually.",
                                nameof(LogEventLevel.Error),
                                "DIAPI_Services",
                                StackExtensions.GetCurrentMethod(),
                                 "Bast",
                                 "HAL 9000");
                        }

                        if (!Double.TryParse(line.ItemPrice.Amount, out double itemPrice))
                        {
                            throw new InvalidOperationException("Invalid ItemPrice value for " + line.SellerSKU + " - " + line.Title);
                        }
                        else
                        {
                            totalItemPrice += itemPrice;
                        }

                        // Amazon ItemPrice = Quantity * ItemPrice
                        DIAPI_Objects_FBA.Invoice.Lines.LineTotal = itemPrice;
                        DIAPI_Objects_FBA.Invoice.Lines.Add();

                        partFound = true;

                        _orderLogRepository.LogEvent(
                            o.Order.AmazonOrderId,
                            String.Format("{0} was added to Invoice", sku),
                            nameof(LogEventLevel.Information),
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                             "Bast",
                             "HAL 9000");
                    }
                    else // SKU not found
                    {
                        // update return Dictionary with missingSKU flag
                        if (!InvoiceNums.ContainsKey("ActionRequired"))
                            InvoiceNums.Add("ActionRequired", "true");
                        else
                            InvoiceNums["ActionRequired"] = "true";

                        _orderLogRepository.LogEvent(
                            o.Order.AmazonOrderId,
                            String.Format("{0} was not found. Please add manually", sku),
                            nameof(LogEventLevel.Error),
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                             "Bast",
                             "HAL 9000");
                    }
                }
                rs = null;

                if (!partFound)
                {
                    DIAPI_Objects_FBA.Invoice.Lines.ItemCode = "PLCHLDR";
                    DIAPI_Objects_FBA.Invoice.Lines.ItemDescription = "Placeholder";
                    DIAPI_Objects_FBA.Invoice.Lines.Quantity = 1;
                    DIAPI_Objects_FBA.Invoice.Lines.UnitPrice = 0;
                    DIAPI_Objects_FBA.Invoice.Lines.Add();

                    _orderLogRepository.LogEvent(
                        o.Order.AmazonOrderId,
                        "Unable to find SKUs for invoice, so a placeholder was added.",
                        nameof(LogEventLevel.Error),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");
                }

                if (!Double.TryParse(o.Order.OrderTotal.Amount, out double total))
                {
                    throw new InvalidOrderTotalException("The OrderTotal for Amazon order " + o.Order.AmazonOrderId + " is invalid.");
                }

                // Override OrderTotal
                // if shipping state is not CA, ignore tax and generate order total from the sum of all totalItemPrice + totalShipCost
                if (string.Equals(o.Order.ShippingAddress.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) || string.Equals(o.Order.ShippingAddress.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase))
                {
                    DIAPI_Objects_FBA.Invoice.DocTotal = total;
                }
                else
                {
                    DIAPI_Objects_FBA.Invoice.DocTotal = totalItemPrice + totalShipCost;
                }

                RetCode = DIAPI_Objects_FBA.Invoice.Add();

                if (RetCode != 0)
                {
                    DIAPI_Objects_FBA.Company.GetLastError(out errCode, out errMsg);

                    _orderLogRepository.LogEvent(
                        o.Order.AmazonOrderId,
                        errCode + " - " + errMsg + " - Invoice was not created.",
                        nameof(LogEventLevel.Critical),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");

                    if (!InvoiceNums.ContainsKey("ActionRequired"))
                        InvoiceNums.Add("ActionRequired", "true");
                    else
                        InvoiceNums["ActionRequired"] = "true";

                    DocEntry = RetCode.ToString(); // return err num
                }
                else
                {
                    // return DocEntry of new Invoice
                    DIAPI_Objects_FBA.Company.GetNewObjectCode(out DocEntry);

                    // NOTE: pass DocEntry to proc to get invoice DocNum for log, but return DocEntry
                    q = "usp_MarketplaceGetDocNum " + DocEntry + ", 'Inv'";
                    rs = (Recordset)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(q);
                    if (!(rs.EoF))
                    {
                        DocNum = Convert.ToInt32(rs.Fields.Item(0).Value);
                    }

                    InvoiceNums.Add("DocEntry", DocEntry);
                    InvoiceNums.Add("DocNum", DocNum.ToString());

                    _orderLogRepository.LogEvent(
                        o.Order.AmazonOrderId,
                        String.Format("Invoice {0} was created. Please verify addresses, pricing, warehouses and shipping.", DocNum),
                        nameof(LogEventLevel.Information),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");

                    rs = null;
                }

                _unitOfWork.Commit();

                // return Dictionary with DocNum and DocEntry
                return InvoiceNums;
            }
            catch (Exception ex)
            {
                _orderLogRepository.LogEvent(
                    o.Order.AmazonOrderId,
                    String.Format("{0} - Invoice was not created.", ex.GetType().Name + " - " + ex.Message),
                    nameof(LogEventLevel.Critical),
                    "DIAPI_Services",
                    StackExtensions.GetCurrentMethod(),
                    "Bast",
                    "HAL 9000");

                if (!InvoiceNums.ContainsKey("ActionRequired"))
                    InvoiceNums.Add("ActionRequired", "true");
                else
                    InvoiceNums["ActionRequired"] = "true";

                rs = null;

                return InvoiceNums;
            }
        }

        /// <summary>
        /// Creates an incoming payment.
        /// </summary>
        /// <param name="cardCode">The BP's CardCode.</param>
        /// <param name="invDocEntry">The Invoice's DocEntry</param>
        /// <param name="order">The Amazon order object</param>
        /// <returns>Payment DocNum on success, -1 on error.</returns>
        public string CreatePayment(string cardCode, string invDocEntry, MWSOrder order)
        {
            Recordset rs;
            string DocEntry = null, DocNum = null, q;

            try
            {
                // If incoming payment exists with AmzOrderId, log and return
                q = "usp_MarketplaceGetIncomingPaymentByAmazonOrderID '" + order.Order.AmazonOrderId + "'";
                rs = (Recordset)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(q);
                if (!(rs.EoF)) // Invoice exists
                {
                    _orderLogRepository.LogEvent(
                        order.Order.AmazonOrderId,
                        "Incoming Payment #" + rs.Fields.Item(0).Value.ToString() + " already exists in SAP.",
                        nameof(LogEventLevel.Information),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");

                    return rs.Fields.Item(0).Value.ToString();
                }
                rs = null;

                DIAPI_Objects_FBA.Payment = (Payments)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.oIncomingPayments);
                DIAPI_Objects_FBA.Payment.Series = 12;
                DIAPI_Objects_FBA.Payment.CardCode = cardCode; // Mandatory
                DIAPI_Objects_FBA.Payment.CashSum = 0; // Mandatory (DocTotal Invoice)
                DIAPI_Objects_FBA.Payment.DocDate = DateTime.Now;

                // convert docEntry to an integer
                if (int.TryParse(invDocEntry, out int docEntry))
                {
                    // Link incoming payment to created invoice
                    DIAPI_Objects_FBA.Payment.Invoices.DocEntry = docEntry;
                }
                else
                {
                    throw new InvalidOperationException("Invoice DocEntry for Amz order " + order.Order.AmazonOrderId + " is not in a valid format.");
                }

                // Bank Transfer
                DIAPI_Objects_FBA.Payment.TransferAccount = "_SYS00000001076";
                DIAPI_Objects_FBA.Payment.TransferDate = DateTime.Now;
                DIAPI_Objects_FBA.Payment.TransferReference = order.Order.AmazonOrderId;

                // if shipping state is CA, use OrderTotal (includes tax and freight).
                if (string.Equals(order.Order.ShippingAddress.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) || string.Equals(order.Order.ShippingAddress.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase))
                {
                    if (Double.TryParse(order.Order.OrderTotal.Amount, out double orderTotal))
                    {
                        DIAPI_Objects_FBA.Payment.TransferSum = orderTotal;
                    }
                    else
                    {
                        throw new InvalidOperationException("Amazon Order " + order.Order.AmazonOrderId + " total has invalid value. Payment not created.");
                    }
                }
                else // Non-CA, so omit tax. Generate order total from the sum of totalItemPrice + totalShipCost
                {
                    double totalItemPrice = 0, totalShipCost = 0;
                    foreach (var l in order.OrderItems)
                    {
                        var s = l.ShippingPrice == null ? "0" : l.ShippingPrice?.Amount;
                        //var t = l.ShippingTax == null ? "0" : l.ShippingTax?.Amount;

                        if (Double.TryParse(s, out double shipCost))
                        {
                            totalShipCost += shipCost;
                            //if (Double.TryParse(t, out double shipTax))
                            //{
                            //    totalShipTax += shipTax;
                            //}
                        }

                        if (!Double.TryParse(l.ItemPrice.Amount, out double itemPrice))
                        {
                            throw new InvalidOperationException("Invalid ItemPrice value for " + l.SellerSKU + " - " + l.Title);
                        }
                        else
                        {
                            totalItemPrice += itemPrice;
                        }
                    }
                    DIAPI_Objects_FBA.Payment.TransferSum = totalItemPrice + totalShipCost;
                }

                RetCode = DIAPI_Objects_FBA.Payment.Add();

                if (RetCode != 0)
                {
                    DIAPI_Objects_FBA.Company.GetLastError(out errCode, out errMsg);

                    _orderLogRepository.LogEvent(
                        order.Order.AmazonOrderId,
                        errCode + " - " + errMsg + " - Incoming payment was not created.",
                        nameof(LogEventLevel.Critical),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");

                    DocNum = RetCode.ToString(); // return err num
                }
                else
                {
                    // return DocEntry of new payment
                    DIAPI_Objects_FBA.Company.GetNewObjectCode(out DocEntry);

                    // NOTE: pass DocEntry to proc to get payment DocNum for log
                    q = "usp_MarketplaceGetDocNum " + DocEntry + ", 'Pmt'";
                    rs = (Recordset)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(q);
                    if (!(rs.EoF))
                    {
                        DocNum = rs.Fields.Item(0).Value.ToString();
                    }

                    _orderLogRepository.LogEvent(
                        order.Order.AmazonOrderId,
                        String.Format("Incoming Payment {0} was created.", DocNum),
                        nameof(LogEventLevel.Information),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");

                    rs = null;
                }

                _unitOfWork.Commit();

                return DocNum;
            }
            catch (Exception ex)
            {
                _orderLogRepository.LogEvent(
                        order.Order.AmazonOrderId,
                        ex.GetType().Name + " - " + ex.Message + " - Incoming payment was not created.",
                        nameof(LogEventLevel.Critical),
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                         "Bast",
                         "HAL 9000");

                return "-1"; // error
            }
        }

        private string GetNextID(string type)
        {
            string ID = string.Empty;

            Recordset rs = (Recordset)DIAPI_Objects_FBA.Company.GetBusinessObject(BoObjectTypes.BoRecordset);

            if (type == "BusinessPartner")
            {
                rs.DoQuery(Constants.SapDiapiSettings.NEXT_CARDCODE_QUERY);
            }
            else if (type == "Order")
            {
                rs.DoQuery(Constants.SapDiapiSettings.NEXT_ORDER_DOCNUM_QUERY);
            }

            while (!(rs.EoF))
            {
                ID = rs.Fields.Item(0).Value.ToString();
                rs.MoveNext();
            }
            rs = null;
            return ID;
        }
    }
}