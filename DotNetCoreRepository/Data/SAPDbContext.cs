using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DotNetCoreRepository.Models;

namespace DotNetCoreRepository.Data
{
    public class SAPDbContext : DbContext
    {
        public SAPDbContext(DbContextOptions<SAPDbContext> options)
            : base(options)
        {
        }

        public DbSet<BusinessPartner> BusinessPartner { get; set; }
        public DbSet<BPAddress> Addresses { get; set; }
        public DbSet<AddressState> StateCode { get; set; }
        public DbSet<ContactName> ContactName { get; set; }
        public DbSet<SerialNumber> SerialNumbers { get; set; }
    }
}
