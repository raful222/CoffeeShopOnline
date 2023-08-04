namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class item : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Items",
                c => new
                    {
                        ItemId = c.Guid(nullable: false),
                        CatogoryId = c.Int(nullable: false),
                        ItemCode = c.String(),
                        ItemName = c.String(),
                        Decription = c.String(),
                        ImagePath = c.String(),
                        ItemPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Category_CategoryId = c.Int(),
                    })
                .PrimaryKey(t => t.ItemId)
                .ForeignKey("dbo.Categories", t => t.Category_CategoryId)
                .Index(t => t.Category_CategoryId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Items", "Category_CategoryId", "dbo.Categories");
            DropIndex("dbo.Items", new[] { "Category_CategoryId" });
            DropTable("dbo.Items");
        }
    }
}
