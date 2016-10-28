namespace Garage20.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Required3 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.VehicleCategories", "Category", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.VehicleCategories", "Category", c => c.String());
        }
    }
}
