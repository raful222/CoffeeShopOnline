namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TableSitTimeEnd", c => c.DateTime(nullable: false));
            DropColumn("dbo.Orders", "TableSitTime");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "TableSitTime", c => c.String());
            DropColumn("dbo.Orders", "TableSitTimeEnd");
        }
    }
}
