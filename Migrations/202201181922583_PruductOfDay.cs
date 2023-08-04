namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PruductOfDay : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Items", "PruductOfDay", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Items", "PruductOfDay");
        }
    }
}
