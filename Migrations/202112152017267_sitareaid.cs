namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sitareaid : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RoomTables", "SitArea_Id", "dbo.SitAreas");
            DropIndex("dbo.RoomTables", new[] { "SitArea_Id" });
            AddColumn("dbo.RoomTables", "SitArea_Id1", c => c.Int());
            AlterColumn("dbo.RoomTables", "SitArea_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.RoomTables", "SitArea_Id1");
            AddForeignKey("dbo.RoomTables", "SitArea_Id1", "dbo.SitAreas", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoomTables", "SitArea_Id1", "dbo.SitAreas");
            DropIndex("dbo.RoomTables", new[] { "SitArea_Id1" });
            AlterColumn("dbo.RoomTables", "SitArea_Id", c => c.Int());
            DropColumn("dbo.RoomTables", "SitArea_Id1");
            CreateIndex("dbo.RoomTables", "SitArea_Id");
            AddForeignKey("dbo.RoomTables", "SitArea_Id", "dbo.SitAreas", "Id");
        }
    }
}
