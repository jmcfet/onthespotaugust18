namespace DataAccessLayer.StoreDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class GiftCardType
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GiftCardTypeID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GiftClassID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GiftClass { get; set; }

        [StringLength(80)]
        public string Description { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "money")]
        public decimal Amount { get; set; }

        [Key]
        [Column(Order = 4, TypeName = "money")]
        public decimal Maximum { get; set; }

        [Key]
        [Column(Order = 5)]
        public bool Enabled { get; set; }

        [Key]
        [Column(Order = 6)]
        public bool IsNotListed { get; set; }

        [Key]
        [Column(Order = 7)]
        public bool IsAwarded { get; set; }

        [Key]
        [Column(Order = 8)]
        public bool ListInCashOut { get; set; }

        [Key]
        [Column(Order = 9)]
        public bool IsBirthday { get; set; }

        [Key]
        [Column(Order = 10)]
        public bool IsPrepaid { get; set; }

        [Key]
        [Column(Order = 11)]
        public bool IsSetAmount { get; set; }

        [Key]
        [Column(Order = 12)]
        public bool IsDynamicAmount { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SortBy { get; set; }

        [Key]
        [Column(Order = 14)]
        public bool Deleted { get; set; }
    }
}
