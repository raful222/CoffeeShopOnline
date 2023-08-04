namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class orderTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TableTime", c => c.DateTime(nullable: false));
            AddColumn("dbo.Orders", "TableNumber", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "TableNumber");
            DropColumn("dbo.Orders", "TableTime");
        }
    }
}
