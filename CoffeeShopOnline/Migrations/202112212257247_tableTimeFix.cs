namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tableTimeFix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TableSitTime", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "TableSitTime");
        }
    }
}
