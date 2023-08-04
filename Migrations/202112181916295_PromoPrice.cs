namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PromoPrice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "PromoPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "PromoPrice");
        }
    }
}
