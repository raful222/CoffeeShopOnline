namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class popular11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "popular", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "popular");
        }
    }
}
