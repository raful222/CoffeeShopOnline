namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dropsitaareaid : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RoomTables", "SitArea_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RoomTables", "SitArea_Id", c => c.Int(nullable: false));
        }
    }
}
