using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class MissingItems
    {
        public string store { get; set; }
        public string description { get; set; }
        public string CustomerName { get; set; }
        public string qcsType { get; set; }
        public string rfid { get; set; }
        public string HeatSeal { get; set; }
        public DateTime? TimeStampIn { get; set; }
        public DateTime? TimeStampOut { get; set; }
    }
}
