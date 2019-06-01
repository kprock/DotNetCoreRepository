using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class ContactName
    {
        [Key]
        public string DefaultContactName { get; set; }
    }
}
