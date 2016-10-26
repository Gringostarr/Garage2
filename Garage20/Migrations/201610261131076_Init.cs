namespace Garage20.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Members",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.VehicleCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Category = c.String(),
                        Size = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Vehicles", "VehicleCategoryId", c => c.Int(nullable: false));
            AddColumn("dbo.Vehicles", "MemberId", c => c.Int(nullable: false));
            CreateIndex("dbo.Vehicles", "VehicleCategoryId");
            CreateIndex("dbo.Vehicles", "MemberId");
            AddForeignKey("dbo.Vehicles", "MemberId", "dbo.Members", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Vehicles", "VehicleCategoryId", "dbo.VehicleCategories", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Vehicles", "VehicleCategoryId", "dbo.VehicleCategories");
            DropForeignKey("dbo.Vehicles", "MemberId", "dbo.Members");
            DropIndex("dbo.Vehicles", new[] { "MemberId" });
            DropIndex("dbo.Vehicles", new[] { "VehicleCategoryId" });
            DropColumn("dbo.Vehicles", "MemberId");
            DropColumn("dbo.Vehicles", "VehicleCategoryId");
            DropTable("dbo.VehicleCategories");
            DropTable("dbo.Members");
        }
    }
}
