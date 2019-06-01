using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.Models
{
    public class FBAOrderLog
    {
        [Key]
        public int OrderLogID { get; set; }

        /// <summary>
        /// AmazonOrderID
        /// </summary>
        public string OrderNo { get; set; }

        public string EventType { get; set; }

        public string EventDescription { get; set; }

        public string ContentRootName { get; set; }

        public string Source { get; set; }

        public string Action { get; set; }

        public string UserName { get; set; }

        public DateTime EventDate { get; set; }
    }
}
