namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quantity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "Quantity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "Quantity");
        }
    }
}
