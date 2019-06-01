using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class AmazonOrderSettings
    {
        [Key]
        public int Id { get; set; }

        public int? ProcessorGroupCount { get; set; }
    }
}
