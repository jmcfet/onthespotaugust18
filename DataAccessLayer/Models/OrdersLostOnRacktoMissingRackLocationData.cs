﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class OrdersLostOnRacktoMissingRackLocationData
    {
        
        public int invoiceID { get; set; }
        public int storeID { get; set; }
        public DateTime? dueDate { get; set; }
    }
}