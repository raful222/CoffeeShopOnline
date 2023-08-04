namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sitarea : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RoomTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TableNumber = c.Int(nullable: false),
                        TableSits = c.Int(nullable: false),
                        Available = c.Boolean(nullable: false),
                        SitArea_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.SitAreas", t => t.SitArea_Id)
                .Index(t => t.SitArea_Id);
            
            CreateTable(
                "dbo.SitAreas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoomType = c.String(),
                        NumberOfSits = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RoomTables", "SitArea_Id", "dbo.SitAreas");
            DropIndex("dbo.RoomTables", new[] { "SitArea_Id" });
            DropTable("dbo.SitAreas");
            DropTable("dbo.RoomTables");
        }
    }
}
