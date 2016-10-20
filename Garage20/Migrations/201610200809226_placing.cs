namespace Garage20.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class placing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Vehicles", "Placing", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Vehicles", "Placing");
        }
    }
}
