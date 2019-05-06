namespace DataAccessLayer.BCSDb
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class View_1
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(150)]
        public string MigrationId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(300)]
        public string ContextKey { get; set; }

        [Key]
        [Column(Order = 2)]
        public byte[] Model { get; set; }

        [Key]
        [Column(Order = 3)]
        [StringLength(32)]
        public string ProductVersion { get; set; }
    }
}
