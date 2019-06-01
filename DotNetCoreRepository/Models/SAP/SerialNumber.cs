using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class SerialNumber
    {
        [Key]
        public string SuppSerial { get; set; }
    }
}
