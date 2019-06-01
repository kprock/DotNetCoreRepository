using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class AddressState
    {
        [Key]
        public string StateCode { get; set; }
    }
}
