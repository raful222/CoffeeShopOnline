namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stars : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "stars", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "stars");
        }
    }
}
