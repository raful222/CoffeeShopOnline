namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NumberOfTaken : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoomTables", "NumberOfTaken", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.RoomTables", "NumberOfTaken");
        }
    }
}
