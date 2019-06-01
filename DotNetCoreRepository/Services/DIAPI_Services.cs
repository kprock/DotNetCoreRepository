using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Diagnostics;
using DotNetCoreRepository.DAL;
using DotNetCoreRepository.Enums;
using DotNetCoreRepository.Extensions;
using DotNetCoreRepository.Models;
using SAPbobsCOM;

namespace DotNetCoreRepository.Services
{
    static class DIAPI_Objects
    {
        public static Company Company;
        public static BusinessPartners BusinessPartner;
        public static Documents Order;
        public static Payments Payment;

        /// <summary>
        /// Connect to SAP Company and Db
        /// </summary>
        /// <returns>Log object.</returns>
        public static AmazonOrderLog Connect()
        {
            if (Company != null)
            {
                if (Company.Connected)
                {
                    var l = new AmazonOrderLog
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

                    var l = new AmazonOrderLog
                    {
                        EventDate = DateTime.Now,
                        EventDescription = errCode + " - " + errMsg,
                        EventType = nameof(LogEventLevel.Error),
                        ContentRootName = "Bast",
                        Source = "DIAPI_Objects",
                        Action = StackExtensions.GetCurrentMethod(),
                        UserName = "SHODAN"
                    };

                    return l;
                }

                if (Company.Connected)
                {
                    var l = new AmazonOrderLog
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
                var l = new AmazonOrderLog
                {
                    EventDate = DateTime.Now,
                    EventDescription = ex.GetType().Name + " - " + ex.Message,
                    EventType = nameof(LogEventLevel.Critical),
                    ContentRootName = "Bast",
                    Source = "DIAPI_Objects",
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

    public class DIAPI_Services : IDIAPI_Services
    {
        private long RetCode;
        private int errCode;
        private string errMsg;

        private readonly IUnitOfWork _unitOfWorkMarketplace;
        private readonly IAmazonOrderLogRepository _amazonOrderLogRepository;
        private readonly ISAPDataService _sapDataService;

        public DIAPI_Services(
            IUnitOfWork unitOfWorkMarketplace,
            IAmazonOrderLogRepository amazonOrderLogRepository,
            ISAPDataService sapDataService
            )
        {
            _unitOfWorkMarketplace = unitOfWorkMarketplace;
            _amazonOrderLogRepository = amazonOrderLogRepository;
            _sapDataService = sapDataService;
        }

        /// <summary>
        /// Creates a BP.
        /// </summary>
        /// <param name="o">The Amazon order object.</param>
        /// <returns>The BP's CardCode or null on error.</returns>
        public string CreateBusinessPartner(AmazonOrder o)
        {
            try
            {
                // If state is longer than two characters, find abbreviation and update Amazon order.
                // If unable to resolve state, throw exception.
                string stateCode = null;
                if (o.StateOrRegion?.Length > 2) // potentially full name of state
                {
                    var s = _sapDataService.GetStateCode(o.StateOrRegion);
                    stateCode = s.StateCode;
                    if (!s.StateCode.IsNullOrWhitespace()) // abbreviation found: Set Amazon-order stateCode to abbreviation
                    {
                        o.StateOrRegion = stateCode;
                    }
                    else // unable to find stateCode. Allow BP creation to continue (allowing for correction), even though invoice creation will fail.
                    {
                        o.StateOrRegion = null;

                        _amazonOrderLogRepository.LogEvent(
                            o.AmazonOrderId,
                            "Shipping state could not be resolved. Please update manually, as invoice creation will fail.",
                            nameof(LogEventLevel.Error),
                            "Bast",
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                            "HAL 9000");
                    }
                }

                // Get Matching BPs from SAP for user's email.
                var bp = _sapDataService.GetCardCodeForEmail(o.BuyerEmail?.Trim());

                if (bp.Count > 0) // Matching BP found. Grab first CardCode in list (most recent Sales Order date) and return.
                {
                    if (bp.Count > 1) // At least one dupe BP. Deactivate.
                    {
                        int i = _sapDataService.DeactivateRedundantBPs(o.BuyerEmail?.Trim(), bp[0].CardCode);
                    }

                    // Add both shipping and billing addresses using the Amazon order's Shipping address
                    var addr = new BPAddress
                    {
                        AddressName = o.AddressName,
                        Street = o.AddressLine1,
                        Block = o.AddressLine2,
                        City = o.City,
                        StateCode = o.StateOrRegion,
                        ZipCode = o.PostalCode,
                        CountryCode = o.CountryCode,
                        PhoneNumber = o.Phone
                    };

                    _sapDataService.AddAddress(bp[0].CardCode, 1, addr); // shipping
                    _sapDataService.AddAddress(bp[0].CardCode, 2, addr); // billing

                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        "BP " + bp[0].CardCode + " already exists. Billing and shipping addresses updated.",
                        nameof(LogEventLevel.Information),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    _unitOfWorkMarketplace.Commit();

                    return bp[0].CardCode;
                }

                // create  new Business Partner
                DIAPI_Objects.BusinessPartner = (BusinessPartners)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.oBusinessPartners);
                DIAPI_Objects.BusinessPartner.CardName = o.BuyerName;
                DIAPI_Objects.BusinessPartner.CardType = BoCardTypes.cCustomer;
                DIAPI_Objects.BusinessPartner.Phone1 = o.Phone;
                DIAPI_Objects.BusinessPartner.EmailAddress = !o.BuyerEmail.IsNullOrWhitespace() ? o.BuyerEmail : "missing.email@fakedomain.com";
                DIAPI_Objects.BusinessPartner.ContactPerson = o.BuyerName;
                DIAPI_Objects.BusinessPartner.Notes = "Auto-created by Marketplace App";
                DIAPI_Objects.BusinessPartner.PayTermsGrpCode = 5; // Credit Card
                DIAPI_Objects.BusinessPartner.Valid = BoYesNoEnum.tYES;

                DIAPI_Objects.BusinessPartner.Addresses.Add();

                // Shipping
                DIAPI_Objects.BusinessPartner.Addresses.SetCurrentLine(0);
                DIAPI_Objects.BusinessPartner.Addresses.AddressType = BoAddressType.bo_ShipTo;
                DIAPI_Objects.BusinessPartner.Addresses.AddressName = o.AddressName;
                DIAPI_Objects.BusinessPartner.Addresses.Street = o.AddressLine1;
                DIAPI_Objects.BusinessPartner.Addresses.Block = o.AddressLine2;
                DIAPI_Objects.BusinessPartner.Addresses.City = o.City;
                DIAPI_Objects.BusinessPartner.Addresses.State = o.StateOrRegion;
                DIAPI_Objects.BusinessPartner.Addresses.ZipCode = o.PostalCode;
                DIAPI_Objects.BusinessPartner.Addresses.Country = o.CountryCode;
                DIAPI_Objects.BusinessPartner.Addresses.UserFields.Fields.Item("U_MarketingOptOut").Value = 0;

                // Billing
                DIAPI_Objects.BusinessPartner.Addresses.SetCurrentLine(1);
                DIAPI_Objects.BusinessPartner.Addresses.AddressType = BoAddressType.bo_BillTo;
                DIAPI_Objects.BusinessPartner.Addresses.AddressName = o.AddressName;
                DIAPI_Objects.BusinessPartner.Addresses.Street = o.AddressLine1;
                DIAPI_Objects.BusinessPartner.Addresses.Block = o.AddressLine2;
                DIAPI_Objects.BusinessPartner.Addresses.City = o.City;
                DIAPI_Objects.BusinessPartner.Addresses.State = o.StateOrRegion;
                DIAPI_Objects.BusinessPartner.Addresses.ZipCode = o.PostalCode;
                DIAPI_Objects.BusinessPartner.Addresses.Country = o.CountryCode;
                DIAPI_Objects.BusinessPartner.Addresses.UserFields.Fields.Item("U_MarketingOptOut").Value = 0;

                DIAPI_Objects.BusinessPartner.CardCode = GetNextID("BusinessPartner"); ;

                // contact info
                DIAPI_Objects.BusinessPartner.ContactEmployees.Add();
                DIAPI_Objects.BusinessPartner.ContactEmployees.SetCurrentLine(0);
                DIAPI_Objects.BusinessPartner.ContactEmployees.Name = o.BuyerName;
                DIAPI_Objects.BusinessPartner.ContactEmployees.Address = o.AddressLine1 + ", " + o.City + ", " + o.StateOrRegion + " " + o.PostalCode + " " + o.CountryCode;
                DIAPI_Objects.BusinessPartner.ContactEmployees.E_Mail = o.BuyerEmail;
                DIAPI_Objects.BusinessPartner.ContactEmployees.Phone1 = o.Phone;

                RetCode = DIAPI_Objects.BusinessPartner.Add();

                // Save CardCode to log and update OrderHeader and UserAccount
                string CardCode = DIAPI_Objects.BusinessPartner.CardCode;

                if (RetCode != 0)
                {
                    DIAPI_Objects.Company.GetLastError(out errCode, out errMsg);

                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        errCode + " - " + errMsg + " - BP was not created.",
                        nameof(LogEventLevel.Critical),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");
                }
                else
                {
                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        "BP " + CardCode + " was created.",
                        nameof(LogEventLevel.Information),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");
                }

                _unitOfWorkMarketplace.Commit();
                return CardCode;
            }
            catch (Exception ex)
            {
                _amazonOrderLogRepository.LogEvent(
                    o.AmazonOrderId,
                    ex.GetType().Name + " - " + ex.Message,
                    nameof(LogEventLevel.Critical),
                    "Bast",
                    "DIAPI_Services",
                    StackExtensions.GetCurrentMethod(),
                    "HAL 9000");

                _unitOfWorkMarketplace.Commit();

                return null;
            }
        }

        /// <summary>
        /// Create SAP Sales Order
        /// </summary>
        /// <param name="cardCode">The CardCode for the BP.</param>
        /// <param name="o">The Amazon order object</param>
        /// <returns>Dictionary with DocEntry and DocNum of the sales order.</returns>
        public Dictionary<string, string> CreateSalesOrder(string cardCode, AmazonOrder o)
        {
            double totalItemPrice = 0, totalItemTax = 0, totalShipCost = 0, totalShipTax = 0;
            string DocEntry = null, q;
            int DocNum = 0;
            Dictionary<string, string> SalesOrderNums = new Dictionary<string, string>();
            Recordset rs;

            try
            {
                // If sales order exists with AmzOrderId, log and return
                q = "usp_MarketplaceGetOrderDocNumByAmazonOrderID '" + o.AmazonOrderId + "'";
                rs = (Recordset)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(q);
                if (!(rs.EoF)) // sales order exists
                {
                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        "Invoice #" + rs.Fields.Item(0).Value.ToString() + " already exists in SAP.",
                        nameof(LogEventLevel.Warning),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    SalesOrderNums.Add("DocNum", rs.Fields.Item(0).Value.ToString());
                    SalesOrderNums.Add("DocEntry", rs.Fields.Item(1).Value.ToString());

                    // Check if sales order total matches Amazon order total
                    var validInvTotal = Decimal.TryParse(rs.Fields.Item(2).Value.ToString(), out decimal invTotal);
                    if (Decimal.Compare(invTotal, o.OrderTotal) != 0)
                    {
                        bool shipToWA = false;
                        if (o.StateOrRegion == "WA" || o.StateOrRegion == "Washington")
                            shipToWA = true;

                        _amazonOrderLogRepository.LogEvent(
                            o.AmazonOrderId,
                            (shipToWA ? "Shipped to WA: " : "") + "Sales order total $" + invTotal.ToString() + " doesn't equal Amazon-order total of $" + o.OrderTotal.ToString() + ". Please verify that this isn't an error.",
                            nameof(LogEventLevel.Warning),
                            "Bast",
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                            "HAL 9000");
                    }

                    _unitOfWorkMarketplace.Commit();

                    return SalesOrderNums;
                }
                rs = null;

                DIAPI_Objects.Order = (Documents)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.oOrders);
                DIAPI_Objects.Order.CardCode = cardCode;
                DIAPI_Objects.Order.NumAtCard = o.AmazonOrderId.Trim();
                DIAPI_Objects.Order.HandWritten = BoYesNoEnum.tNO;
                DIAPI_Objects.Order.DocDate = DateTime.Now;
                DIAPI_Objects.Order.DocDueDate = DateTime.Now;
                DIAPI_Objects.Order.DocCurrency = "$";
                DIAPI_Objects.Order.SalesPersonCode = 68; // Amazon slpcode
                DIAPI_Objects.Order.UserFields.Fields.Item("U_LeadSource").Value = "29";

                // Calculate shipping
                foreach (var l in o.OrderItems)
                {
                    totalShipCost += Decimal.ToDouble(l.ShippingPrice);
                    //totalShipTax += Decimal.ToDouble(l.ShippingTax);
                }

                DIAPI_Objects.Order.Expenses.ExpenseCode = 1;
                DIAPI_Objects.Order.Expenses.LineTotal = totalShipCost;
                DIAPI_Objects.Order.Expenses.Add();

                bool partFound = false;
                foreach (var line in o.OrderItems)
                {
                    q = "usp_findPartBySKU '" + line.VendorSKU + "'";
                    rs = (Recordset)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(q);
                    if (!(rs.EoF))
                    {
                        DIAPI_Objects.Order.Lines.ItemCode = rs.Fields.Item(0).Value.ToString();
                        DIAPI_Objects.Order.Lines.Quantity = Decimal.ToDouble(line.QuantityOrdered);
                        DIAPI_Objects.Order.Lines.WarehouseCode = "DS1";

                        // Set tax
                        // flag "Shipping to CA, but no sales tax charged"
                        if ((string.Equals(o.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) || string.Equals(o.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase)) && line.ItemTax == 0)
                        {
                            DIAPI_Objects.Order.Lines.TaxCode = "Exempt";
                            DIAPI_Objects.Order.Lines.TaxTotal = 0;

                            _amazonOrderLogRepository.LogEvent(
                                o.AmazonOrderId,
                                "Adding " + line.VendorSKU + " - Shipping address is CA, but no sales tax was charged.",
                                nameof(LogEventLevel.Information),
                                "Bast",
                                "DIAPI_Services",
                                StackExtensions.GetCurrentMethod(),
                                "HAL 9000");
                        }
                        else if (!string.Equals(o.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) && !string.Equals(o.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase))
                        {
                            // no tax outside of california
                            DIAPI_Objects.Order.Lines.TaxCode = "Exempt";
                            DIAPI_Objects.Order.Lines.TaxTotal = 0;
                        }
                        else // California
                        {
                            // Set tax
                            DIAPI_Objects.Order.Lines.TaxCode = "CA";
                            DIAPI_Objects.Order.Lines.TaxTotal = Decimal.ToDouble(line.ItemTax);
                        }

                        totalItemPrice += Decimal.ToDouble(line.ItemPrice); // add ItemPrice to total
                        
                        // Amazon ItemPrice = Quantity * ItemPrice
                        DIAPI_Objects.Order.Lines.LineTotal = Decimal.ToDouble(line.ItemPrice);
                        DIAPI_Objects.Order.Lines.Add();

                        partFound = true;

                        _amazonOrderLogRepository.LogEvent(
                            o.AmazonOrderId,
                            String.Format("{0} was added to sales order", line.VendorSKU),
                            nameof(LogEventLevel.Information),
                            "Bast",
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                            "HAL 9000");
                    }
                    else // SKU not found
                    {
                        // update return Dictionary with 'ActionRequired' flag
                        if (!SalesOrderNums.ContainsKey("ActionRequired"))
                            SalesOrderNums.Add("ActionRequired", "true");
                        else
                            SalesOrderNums["ActionRequired"] = "true";

                        _amazonOrderLogRepository.LogEvent(
                            o.AmazonOrderId,
                            String.Format("{0} was not found. Please add manually", line.VendorSKU),
                            nameof(LogEventLevel.Error),
                            "Bast",
                            "DIAPI_Services",
                            StackExtensions.GetCurrentMethod(),
                            "HAL 9000");
                    }
                }
                rs = null;

                if (!partFound)
                {
                    DIAPI_Objects.Order.Lines.ItemCode = "PLCHLDR";
                    DIAPI_Objects.Order.Lines.ItemDescription = "Placeholder";
                    DIAPI_Objects.Order.Lines.Quantity = 1;
                    DIAPI_Objects.Order.Lines.UnitPrice = 0;
                    DIAPI_Objects.Order.Lines.Add();

                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        "Unable to find SKUs for sales order, so a placeholder was added.",
                        nameof(LogEventLevel.Error),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");
                }

                // Override OrderTotal
                // if shipping state is not CA, ignore tax and generate order total from the sum of all totalItemPrice + totalShipCost
                if (string.Equals(o.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) || string.Equals(o.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase))
                {
                    DIAPI_Objects.Order.DocTotal = Decimal.ToDouble(o.OrderTotal);
                }
                else
                {
                    DIAPI_Objects.Order.DocTotal = totalItemPrice + totalShipCost;
                }

                RetCode = DIAPI_Objects.Order.Add();

                if (RetCode != 0)
                {
                    DIAPI_Objects.Company.GetLastError(out errCode, out errMsg);

                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        errCode + " - " + errMsg + " - Sales order was not created.",
                        nameof(LogEventLevel.Critical),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    if (!SalesOrderNums.ContainsKey("ActionRequired"))
                        SalesOrderNums.Add("ActionRequired", "true");
                    else
                        SalesOrderNums["ActionRequired"] = "true";

                    DocEntry = RetCode.ToString(); // return err num
                }
                else
                {
                    // return DocEntry of new Invoice
                    DIAPI_Objects.Company.GetNewObjectCode(out DocEntry);

                    // NOTE: pass DocEntry to proc to get sales-order DocNum for log, but return DocEntry
                    q = "usp_MarketplaceGetDocNum " + DocEntry + ", 'SO'";
                    rs = (Recordset)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(q);
                    if (!(rs.EoF))
                    {
                        DocNum = Convert.ToInt32(rs.Fields.Item(0).Value);
                    }

                    SalesOrderNums.Add("DocEntry", DocEntry);
                    SalesOrderNums.Add("DocNum", DocNum.ToString());

                    _amazonOrderLogRepository.LogEvent(
                        o.AmazonOrderId,
                        String.Format("Sales order {0} was created. Please verify addresses, pricing, warehouses and shipping.", DocNum),
                        nameof(LogEventLevel.Information),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    rs = null;
                }

                _unitOfWorkMarketplace.Commit();

                // return Dictionary with sales-order DocNum and DocEntry
                return SalesOrderNums;
            }
            catch (Exception ex)
            {
                _amazonOrderLogRepository.LogEvent(
                    o.AmazonOrderId,
                    String.Format("{0} - Sales order was not created.", ex.GetType().Name + " - " + ex.Message),
                    nameof(LogEventLevel.Critical),
                    "Bast",
                    "DIAPI_Services",
                    StackExtensions.GetCurrentMethod(),
                    "HAL 9000");

                if (!SalesOrderNums.ContainsKey("ActionRequired"))
                    SalesOrderNums.Add("ActionRequired", "true");
                else
                    SalesOrderNums["ActionRequired"] = "true";

                rs = null;

                return SalesOrderNums;
            }
        }

        /// <summary>
        /// Creates an incoming payment.
        /// </summary>
        /// <param name="cardCode">The BP's CardCode.</param>
        /// <param name="salesOrderNo">The salesOrder's DocNum</param>
        /// <param name="order">The Amazon order object</param>
        /// <returns>Payment DocNum on success, -1 on error.</returns>
        public string CreatePayment(string cardCode, string salesOrderNo, AmazonOrder order)
        {
            Recordset rs;
            string DocEntry = null, DocNum = null, q;

            try
            {
                // If incoming payment exists with AmzOrderId, log and return
                q = "usp_MarketplaceGetIncomingPaymentByAmazonOrderID '" + order.AmazonOrderId + "'";
                rs = (Recordset)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(q);
                if (!(rs.EoF)) // Invoice exists
                {
                    _amazonOrderLogRepository.LogEvent(
                        order.AmazonOrderId,
                        "Incoming Payment #" + rs.Fields.Item(0).Value.ToString() + " already exists in SAP.",
                        nameof(LogEventLevel.Information),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    return rs.Fields.Item(0).Value.ToString();
                }
                rs = null;

                DIAPI_Objects.Payment = (Payments)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.oIncomingPayments);
                DIAPI_Objects.Payment.Series = 12;
                DIAPI_Objects.Payment.CardCode = cardCode; // Mandatory
                DIAPI_Objects.Payment.CashSum = 0; // Mandatory (DocTotal Invoice)
                DIAPI_Objects.Payment.DocDate = DateTime.Now;
                DIAPI_Objects.Payment.Remarks = order.AmazonOrderId;
                DIAPI_Objects.Payment.CounterReference = salesOrderNo;

                // Bank Transfer
                DIAPI_Objects.Payment.TransferAccount = "_SYS00000001076";
                DIAPI_Objects.Payment.TransferDate = DateTime.Now;
                DIAPI_Objects.Payment.TransferReference = order.AmazonOrderId;

                // if shipping state is CA, use OrderTotal (includes tax and freight).
                if (string.Equals(order.StateOrRegion, "ca", StringComparison.OrdinalIgnoreCase) || string.Equals(order.StateOrRegion, "california", StringComparison.OrdinalIgnoreCase))
                {
                    DIAPI_Objects.Payment.TransferSum = Decimal.ToDouble(order.OrderTotal);
                }
                else // Non-CA, so omit tax. Generate order total from the sum of totalItemPrice + totalShipCost
                {
                    double totalItemPrice = 0, totalShipCost = 0;
                    foreach (var l in order.OrderItems)
                    {
                        totalItemPrice += Decimal.ToDouble(l.ItemPrice);
                        totalShipCost += Decimal.ToDouble(l.ShippingPrice);
                    }
                    DIAPI_Objects.Payment.TransferSum = totalItemPrice + totalShipCost;
                }

                RetCode = DIAPI_Objects.Payment.Add();

                if (RetCode != 0)
                {
                    DIAPI_Objects.Company.GetLastError(out errCode, out errMsg);

                    _amazonOrderLogRepository.LogEvent(
                        order.AmazonOrderId,
                        errCode + " - " + errMsg + " - Incoming payment was not created.",
                        nameof(LogEventLevel.Critical),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    DocNum = RetCode.ToString(); // return err num
                }
                else
                {
                    // return DocEntry of new payment
                    DIAPI_Objects.Company.GetNewObjectCode(out DocEntry);

                    // NOTE: pass DocEntry to proc to get payment DocNum for log
                    q = "usp_MarketplaceGetDocNum " + DocEntry + ", 'Pmt'";
                    rs = (Recordset)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                    rs.DoQuery(q);
                    if (!(rs.EoF))
                    {
                        DocNum = rs.Fields.Item(0).Value.ToString();
                    }

                    _amazonOrderLogRepository.LogEvent(
                        order.AmazonOrderId,
                        String.Format("Incoming Payment {0} was created.", DocNum),
                        nameof(LogEventLevel.Information),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                    rs = null;
                }

                _unitOfWorkMarketplace.Commit();

                return DocNum;
            }
            catch (Exception ex)
            {
                _amazonOrderLogRepository.LogEvent(
                        order.AmazonOrderId,
                        ex.GetType().Name + " - " + ex.Message + " - Incoming payment was not created.",
                        nameof(LogEventLevel.Critical),
                        "Bast",
                        "DIAPI_Services",
                        StackExtensions.GetCurrentMethod(),
                        "HAL 9000");

                return "-1"; // error
            }
        }

        private string GetNextID(string type)
        {
            string ID = string.Empty;

            Recordset rs = (Recordset)DIAPI_Objects.Company.GetBusinessObject(BoObjectTypes.BoRecordset);

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
