namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
/* chose Select ADO.NET Entity Data Model in the Add New Item dialog box and specify the model name */
    public partial class v_Logger
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LogID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StoreID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WorkStationID { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GarmentStoreID { get; set; }

        [Key]
        [Column(Order = 4)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerID { get; set; }

        [StringLength(50)]
        public string CustomerName { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DepartmentID { get; set; }

        [StringLength(50)]
        public string DepartmentName { get; set; }

        [StringLength(20)]
        public string ArticleCode { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployeeID { get; set; }

        public DateTime? TimeStampIn { get; set; }

        public DateTime? TimeStampOut { get; set; }

        [StringLength(1)]
        public string Starch { get; set; }

        [StringLength(1)]
        public string Package { get; set; }

        [StringLength(25)]
        public string Comment { get; set; }

        [StringLength(200)]
        public string FlagTag { get; set; }

        [StringLength(200)]
        public string Reason { get; set; }

        [StringLength(200)]
        public string Notes { get; set; }
    }
}
