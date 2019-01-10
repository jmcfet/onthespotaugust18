namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nullableDate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.QCSInfo", "Time", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.QCSInfo", "Time", c => c.DateTime(nullable: false));
        }
    }
}
