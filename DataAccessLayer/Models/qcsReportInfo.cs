using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class qcsReportInfo
    {
        public int CustomerID { get; set; }
        public int InvoiceNumber { get; set; }
        public string dept { get; set; }
        public DateTime? Duedate { get; set; }
            public DateTime? InvoiceDate { get; set; }
        public int storeid { get; set; }
        public string storeName { get; set; }
        public string rfid { get; set; }
        public string customerID { get; set; }
        public string HeatSeal { get; set; }
        public string Description { get; set; }
        public string qcsType { get; set; }
        public DateTime? TimeStampIn { get; set; }
        public DateTime? TimeStampOut { get; set; }
    }
}
