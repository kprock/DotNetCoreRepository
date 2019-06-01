using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreRepository.Models;
using System.Data.SqlClient;
using System.Text;

namespace DotNetCoreRepository.DAL
{
    class SAPDataService : RepositoryBaseSAP, ISAPDataService
    {
        public SAPDataService(IDatabaseFactorySAP databaseFactory)
            : base(databaseFactory)
        {
        }

        public List<BusinessPartner> GetCardCodeForEmail(string emailAddress)
        {
            // retrieve CardCode and CardName
            List<BusinessPartner> lst = DatabaseSAP.BusinessPartner
                                    .FromSql("usp_PortalGetCardCodeForEmail @p0", emailAddress)
                                    .ToList();
            return lst;
        }

        public int DeactivateRedundantBPs(string emailAddress, string cardCode)
        {
            int i = DatabaseSAP.Database.ExecuteSqlCommand("usp_PortalDeactivateRedundantBPs @p0, @p1",
                        parameters: new[] { emailAddress, cardCode });
            return i;
        }

        public BPAddress AddAddress(string cardCode, int addressType, BPAddress addr)
        {
            var a = DatabaseSAP.Addresses
                        .FromSql("usp_PortalAddSAPAddress @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12",
                            0.ToString(), cardCode, addressType.ToString(), addr.AddressName, addr.Street,
                            addr.Block, addr.City, addr.StateCode, addr.ZipCode, addr.CountryCode, addr.PhoneNumber,
                            0.ToString(), 1.ToString()).FirstOrDefault();
            return a;
        }

        public AddressState GetStateCode(string stateName)
        {
            var s = DatabaseSAP.StateCode
            .FromSql("usp_MarketplaceGetStateAbbr @p0", stateName).FirstOrDefault();

            return s;
        }

        public ContactName GetContactName(string cardCode, string contactName)
        {
            var c = DatabaseSAP.ContactName
            .FromSql("usp_MarketplaceGetContactPerson @p0, @p1", cardCode, contactName).FirstOrDefault();

            return c;
        }

        public List<SerialNumber> GetSerialNumbers(string SKU, int count)
        {
            // retrieve serial numbers
            List<SerialNumber> lst = DatabaseSAP.SerialNumbers
                                    .FromSql("usp_MarketplaceGetSerialsBySKU @p0, @p1", SKU, count.ToString())
                                    .ToList();
            return lst;
        }
    }
}
