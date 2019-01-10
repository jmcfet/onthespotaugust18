namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("QCSInfo")]
    public partial class QCSInfo
    {
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string HeatSeal { get; set; }

        [Required]
        [StringLength(50)]
        public string Bin { get; set; }

        public DateTime? Time { get; set; }
    }
}
