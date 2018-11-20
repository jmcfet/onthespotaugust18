namespace DataAccessLayer
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class BCSContext : DbContext
    {
        public BCSContext()
            : base("name=BCSContext2")
        {
        }

        public virtual DbSet<Appointment> Appointments { get; set; }
        public virtual DbSet<Bin> Bins { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<CPR> CPRs { get; set; }
        public virtual DbSet<GSS> GSSes { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<OTISIdsToFabId> OTISIdsToFabIds { get; set; }
        public virtual DbSet<NoteForCustomer> CustomerNotes { get; set; }
        public virtual DbSet<Printer> Printers { get; set; }
        public virtual DbSet<QCSInfo> QCSInfoes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>()
                .Property(e => e.Subject)
                .IsUnicode(false);

            modelBuilder.Entity<Appointment>()
                .Property(e => e.Location)
                .IsUnicode(false);

            modelBuilder.Entity<Appointment>()
                .Property(e => e.Body)
                .IsUnicode(false);

            modelBuilder.Entity<Appointment>()
                .Property(e => e.temp1)
                .IsUnicode(false);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Bins)
                .WithRequired(e => e.Category1)
                .HasForeignKey(e => e.Category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Category>()
                .HasMany(e => e.Items)
                .WithOptional(e => e.Category)
                .HasForeignKey(e => e.CatID);

            modelBuilder.Entity<CPR>()
                .Property(e => e.store)
                .IsUnicode(false);

            modelBuilder.Entity<GSS>()
                .Property(e => e.temp)
                .IsFixedLength();

            modelBuilder.Entity<GSS>()
                .Property(e => e.temp1)
                .IsFixedLength();

            modelBuilder.Entity<GSS>()
                .Property(e => e.temp2)
                .IsFixedLength();

            modelBuilder.Entity<GSS>()
                .Property(e => e.temp3)
                .IsFixedLength();

            modelBuilder.Entity<Item>()
                .Property(e => e.State)
                .IsFixedLength();

            //modelBuilder.Entity<OTSIDtoFAB>()
            //    .Property(e => e.PriceTable)
            //    .IsFixedLength();
        }
    }
}
