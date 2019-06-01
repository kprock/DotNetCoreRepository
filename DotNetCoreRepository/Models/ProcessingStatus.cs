using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class ProcessingStatus
    {
        [Key]
        public int ProcessingStatusID { get; set; }

        public string Description { get; set; }

        public int SortWeight { get; set; }
    }
}
