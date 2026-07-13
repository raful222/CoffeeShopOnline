namespace CoffeeShopOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PersistentCartDrafts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CartDraftLines",
                c => new
                    {
                        CartDraftLineId = c.Int(nullable: false, identity: true),
                        CartDraftId = c.Guid(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CartDraftLineId)
                .ForeignKey("dbo.CartDrafts", t => t.CartDraftId, cascadeDelete: true)
                .Index(t => new { t.CartDraftId, t.ItemId }, unique: true, name: "IX_CartDraftLine_Item");
            
            CreateTable(
                "dbo.CartDrafts",
                c => new
                    {
                        CartDraftId = c.Guid(nullable: false),
                        CartKey = c.String(nullable: false, maxLength: 32),
                        UserId = c.String(maxLength: 128),
                        TableId = c.Int(),
                        DinerCount = c.Int(),
                        ClosedParty = c.Boolean(nullable: false),
                        UpdatedUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CartDraftId)
                .Index(t => t.CartKey, unique: true, name: "IX_CartDraft_CartKey")
                .Index(t => t.UserId, name: "IX_CartDraft_UserId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CartDraftLines", "CartDraftId", "dbo.CartDrafts");
            DropIndex("dbo.CartDrafts", "IX_CartDraft_UserId");
            DropIndex("dbo.CartDrafts", "IX_CartDraft_CartKey");
            DropIndex("dbo.CartDraftLines", "IX_CartDraftLine_Item");
            DropTable("dbo.CartDrafts");
            DropTable("dbo.CartDraftLines");
        }
    }
}
