using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DotNetCoreRepository  .Models
{
    public class BusinessPartner
    {
        [Key]
        public string CardCode { set; get; }
        public string CardName { get; set; }
        public DateTime DocDate { set; get; }
        public string validFor { set; get; }
    }
}
