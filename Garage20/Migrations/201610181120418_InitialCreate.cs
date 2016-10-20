namespace Garage20.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Vehicles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Regnr = c.String(),
                        Color = c.String(),
                        NumberOfWheels = c.Int(nullable: false),
                        VehicleType = c.Int(nullable: false),
                        Checkin = c.DateTime(nullable: false),
                        Checkout = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Vehicles");
        }
    }
}
