using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class ShirtInfo
    {
       
        public int invoiceID { get; set; }
        public string type { get; set; }
        public string articleID { get; set; }
        public DateTime? dueDate { get; set; }
    }
}
