using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class Processor
    {
        [Key]
        public int ProcessorId { get; set; }

        public string ProcessorName { get; set; }

        public bool Active { get; set; }

        public DateTime? LastAssignDate { get; set; }
    }
}
