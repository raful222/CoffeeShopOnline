namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class outorin : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.RoomTables", "SitArea_Id1", "dbo.SitAreas");
            DropIndex("dbo.RoomTables", new[] { "SitArea_Id1" });
            AddColumn("dbo.RoomTables", "InOrOut", c => c.Boolean(nullable: false));
            DropColumn("dbo.RoomTables", "SitArea_Id1");
            DropTable("dbo.SitAreas");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SitAreas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoomType = c.String(),
                        NumberOfSits = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.RoomTables", "SitArea_Id1", c => c.Int());
            DropColumn("dbo.RoomTables", "InOrOut");
            CreateIndex("dbo.RoomTables", "SitArea_Id1");
            AddForeignKey("dbo.RoomTables", "SitArea_Id1", "dbo.SitAreas", "Id");
        }
    }
}
