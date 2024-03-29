﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnTheSpot.Models
{
    public class AutoSortInfo
    {
        public int CustomerID { get; set; }
        public int InvoiceNumber { get; set; }
        public string dept  { get; set; }
        public DateTime? Duedate { get; set; }
        public int storeid { get; set; }
        public string  storeName { get; set; }
        public string rfid { get; set; }
        public string customerName { get; set; }
        public string HeatSeal { get; set; }
        public string Description { get; set; }
        public int state { get; set; }
        public string slot { get; set; }
    }
}
