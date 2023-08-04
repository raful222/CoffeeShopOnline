namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsAprroved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "IsAprroved", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "IsAprroved");
        }
    }
}
