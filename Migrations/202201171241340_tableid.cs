namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tableid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoomTables", "TableId", c => c.Int(nullable: false));
            DropColumn("dbo.RoomTables", "TableNumber");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RoomTables", "TableNumber", c => c.Int(nullable: false));
            DropColumn("dbo.RoomTables", "TableId");
        }
    }
}
