namespace DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addloogerview : DbMigration
    {
        public override void Up()
        {
            
        }
        
        public override void Down()
        {
            DropTable("dbo.v_Logger");
        }
    }
}
