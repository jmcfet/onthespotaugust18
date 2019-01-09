namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Item
    {
        public int ID { get; set; }

        public int? CustID { get; set; }

        [StringLength(50)]
        public string BarCode { get; set; }

        [StringLength(10)]
        public string State { get; set; }

        public int? CatID { get; set; }

        public DateTime? CreateDate { get; set; }

        public string Note { get; set; }

        public string picture { get; set; }

        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
    }
}
