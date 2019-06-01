using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreRepository.Models;

namespace DotNetCoreRepository.DAL
{
    public interface ISAPDataService
    {
        /// <summary>
        /// Returns CardCode (and other data) of BusinessPartner
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns>A List of Business Partners with the passed email address.</returns>
        List<BusinessPartner> GetCardCodeForEmail(string emailAddress);

        /// <summary>
        /// Makes inactive all duplicate Business Partners with matching email address but different CardCode
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        int DeactivateRedundantBPs(string emailAddress, string cardCode);

        /// <summary>
        /// Adds Business Partner address and makes it default for the address type.
        /// </summary>
        /// <param name="cardCode">The BP's CardCode.</param>
        /// <param name="addressType">1 for shipping, 2 for billing</param>
        /// <param name="addr">The Address POCO to add.</param>
        /// <returns>The created address POCO.</returns>
        BPAddress AddAddress(string cardCode, int addressType, BPAddress addr);

        /// <summary>
        /// Gets state abbreviation from the full state name.
        /// </summary>
        /// <param name="stateName">The full name of the state.</param>
        /// <returns>The state abbreviation.</returns>
        AddressState GetStateCode(string stateName);

        /// <summary>
        /// Gets contact name matching CardCode and Name.
        /// </summary>
        /// <param name="cardCode">The business partner's CardCode.</param>
        /// <param name="contactName">Name of the contact.</param>
        /// <returns>The full contact name.</returns>
        ContactName GetContactName(string cardCode, string contactName);

        /// <summary>
        /// Gets available serial numbers for SKU and item count
        /// </summary>
        /// <param name="SKU">The SAP ItemCode.</param>
        /// <param name="count">The number of serials requested.</param>
        /// <returns>A list object with specified number of serials</returns>
        List<SerialNumber> GetSerialNumbers(string SKU, int count);
    }
}
