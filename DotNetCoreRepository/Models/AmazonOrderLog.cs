using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class AmazonOrderLog
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
