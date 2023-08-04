namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sitar : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoomTables", "roomType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RoomTables", "roomType");
        }
    }
}
