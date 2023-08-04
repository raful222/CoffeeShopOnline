namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class promo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "Promo", c => c.Boolean(nullable: false));
        }
          
        public override void Down()
        {
            DropColumn("dbo.Items", "Promo");
        }
    }
}
