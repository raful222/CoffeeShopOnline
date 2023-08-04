namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tablenumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoomTables", "TableNumber", c => c.Int(nullable: false));
            DropColumn("dbo.RoomTables", "TableId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.RoomTables", "TableId", c => c.Int(nullable: false));
            DropColumn("dbo.RoomTables", "TableNumber");
        }
    }
}
