namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dropdate : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Orders", "TableTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "TableTime", c => c.DateTime(nullable: false));
        }
    }
}
