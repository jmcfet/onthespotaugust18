namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class v_Loggerall
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int LogID { get; set; }

        public DateTime? LogDate { get; set; }

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
        public int LaborID { get; set; }

        [StringLength(12)]
        public string LaborCode { get; set; }

        [StringLength(50)]
        public string LaborName { get; set; }

        [Key]
        [Column(Order = 5)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ProcessID { get; set; }

        [StringLength(50)]
        public string ProcessName { get; set; }

        [Key]
        [Column(Order = 6)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerID { get; set; }

        [StringLength(50)]
        public string CustomerName { get; set; }

        [Key]
        [Column(Order = 7)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DepartmentID { get; set; }

        [StringLength(50)]
        public string DepartmentName { get; set; }

        [StringLength(32)]
        public string DetailGuid { get; set; }

        [Key]
        [Column(Order = 8)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int GarmentID { get; set; }

        [StringLength(50)]
        public string GarmentName { get; set; }

        [StringLength(20)]
        public string ArticleCode { get; set; }

        [StringLength(200)]
        public string InvoiceLine { get; set; }

        public DateTime? MarkInDate { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MarkInEmpID { get; set; }

        [Key]
        [Column(Order = 10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EmployeeID { get; set; }

        [Key]
        [Column(Order = 11, TypeName = "money")]
        public decimal Amount { get; set; }

        [Key]
        [Column(Order = 12)]
        public float CommCost { get; set; }

        [Key]
        [Column(Order = 13)]
        public float LaborCost { get; set; }

        [Key]
        [Column(Order = 14)]
        public float TimeCost { get; set; }

        [Key]
        [Column(Order = 15)]
        public float PieceCost { get; set; }

        [Key]
        [Column(Order = 16)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int State { get; set; }

        [Key]
        [Column(Order = 17)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TimeMode { get; set; }

        [Key]
        [Column(Order = 18)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CostMode { get; set; }

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
