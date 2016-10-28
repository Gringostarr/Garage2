namespace Garage20.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Requiredfileds : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Vehicles", "Regnr", c => c.String(nullable: false));
            AlterColumn("dbo.Vehicles", "Color", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Vehicles", "Color", c => c.String());
            AlterColumn("dbo.Vehicles", "Regnr", c => c.String());
        }
    }
}
