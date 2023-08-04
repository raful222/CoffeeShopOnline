namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NumOfDiners : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "NumberOfDiners", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "NumberOfDiners");
        }
    }
}
