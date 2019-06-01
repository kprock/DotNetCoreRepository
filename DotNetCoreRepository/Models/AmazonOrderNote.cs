using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DotNetCoreRepository.Models
{
    public class AmazonOrderNote
    {
        [Key]
        public int AmazonOrderNoteID { get; set; }

        /// <summary>
        /// FK to AmazonOrder entity
        /// </summary>
        public int OrderID { get; set; }

        // 1: Vendor, 2: Customer, 3: Internal 
        public int OrderNoteTypeID { get; set; }

        public string Note { get; set; }

        public string EmployeeName { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual AmazonOrder AmazonOrder { get; set; }
    }
}
