namespace DDrop.Db.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DbContours",
                c => new
                    {
                        ContourId = c.Guid(nullable: false),
                        CalculationParameters = c.String(maxLength: 4000),
                        CalculationProvider = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.ContourId)
                .ForeignKey("dbo.DropPhotos", t => t.ContourId)
                .Index(t => t.ContourId);
            
            CreateTable(
                "dbo.DropPhotos",
                c => new
                    {
                        DropPhotoId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 4000),
                        XDiameterInPixels = c.Int(nullable: false),
                        YDiameterInPixels = c.Int(nullable: false),
                        ZDiameterInPixels = c.Int(nullable: false),
                        SimpleHorizontalLineId = c.Guid(),
                        SimpleVerticalLineId = c.Guid(),
                        Content = c.Binary(maxLength: 4000),
                        AddedDate = c.String(maxLength: 4000),
                        CreationDateTime = c.String(maxLength: 4000),
                        PhotoOrderInSeries = c.Int(nullable: false),
                        CurrentSeriesId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.DropPhotoId)
                .ForeignKey("dbo.Series", t => t.CurrentSeriesId, cascadeDelete: true)
                .ForeignKey("dbo.SimpleLines", t => t.SimpleHorizontalLineId)
                .ForeignKey("dbo.SimpleLines", t => t.SimpleVerticalLineId)
                .Index(t => t.SimpleHorizontalLineId)
                .Index(t => t.SimpleVerticalLineId)
                .Index(t => t.CurrentSeriesId);
            
            CreateTable(
                "dbo.Series",
                c => new
                    {
                        SeriesId = c.Guid(nullable: false),
                        Title = c.String(maxLength: 4000),
                        IntervalBetweenPhotos = c.Double(nullable: false),
                        AddedDate = c.String(maxLength: 4000),
                        UseCreationDateTime = c.Boolean(nullable: false),
                        CurrentUserId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.SeriesId)
                .ForeignKey("dbo.Users", t => t.CurrentUserId, cascadeDelete: true)
                .Index(t => t.CurrentUserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        FirstName = c.String(maxLength: 4000),
                        LastName = c.String(maxLength: 4000),
                        UserPhoto = c.Binary(maxLength: 4000),
                        Password = c.String(maxLength: 4000),
                        Email = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.ReferencePhotos",
                c => new
                    {
                        ReferencePhotoId = c.Guid(nullable: false),
                        Name = c.String(maxLength: 4000),
                        Content = c.Binary(maxLength: 4000),
                        SimpleReferencePhotoLineId = c.Guid(),
                        PixelsInMillimeter = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReferencePhotoId)
                .ForeignKey("dbo.SimpleLines", t => t.SimpleReferencePhotoLineId)
                .ForeignKey("dbo.Series", t => t.ReferencePhotoId)
                .Index(t => t.ReferencePhotoId)
                .Index(t => t.SimpleReferencePhotoLineId);
            
            CreateTable(
                "dbo.SimpleLines",
                c => new
                    {
                        SimpleLineId = c.Guid(nullable: false),
                        X1 = c.Double(nullable: false),
                        Y1 = c.Double(nullable: false),
                        X2 = c.Double(nullable: false),
                        Y2 = c.Double(nullable: false),
                        ContourId = c.Guid(),
                    })
                .PrimaryKey(t => t.SimpleLineId)
                .ForeignKey("dbo.DbContours", t => t.ContourId)
                .Index(t => t.ContourId);
            
            CreateTable(
                "dbo.Drops",
                c => new
                    {
                        DropId = c.Guid(nullable: false),
                        XDiameterInMeters = c.Double(nullable: false),
                        YDiameterInMeters = c.Double(nullable: false),
                        ZDiameterInMeters = c.Double(nullable: false),
                        VolumeInCubicalMeters = c.Double(nullable: false),
                        RadiusInMeters = c.Double(),
                    })
                .PrimaryKey(t => t.DropId)
                .ForeignKey("dbo.DropPhotos", t => t.DropId)
                .Index(t => t.DropId);
            
            CreateTable(
                "dbo.DbLogEntries",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.String(maxLength: 4000),
                        Username = c.String(maxLength: 4000),
                        LogLevel = c.String(maxLength: 4000),
                        LogCategory = c.String(maxLength: 4000),
                        Message = c.String(maxLength: 4000),
                        Details = c.String(maxLength: 4000),
                        Exception = c.String(maxLength: 4000),
                        InnerException = c.String(maxLength: 4000),
                        StackTrace = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SimpleLines", "ContourId", "dbo.DbContours");
            DropForeignKey("dbo.DbContours", "ContourId", "dbo.DropPhotos");
            DropForeignKey("dbo.DropPhotos", "SimpleVerticalLineId", "dbo.SimpleLines");
            DropForeignKey("dbo.DropPhotos", "SimpleHorizontalLineId", "dbo.SimpleLines");
            DropForeignKey("dbo.Drops", "DropId", "dbo.DropPhotos");
            DropForeignKey("dbo.ReferencePhotos", "ReferencePhotoId", "dbo.Series");
            DropForeignKey("dbo.ReferencePhotos", "SimpleReferencePhotoLineId", "dbo.SimpleLines");
            DropForeignKey("dbo.DropPhotos", "CurrentSeriesId", "dbo.Series");
            DropForeignKey("dbo.Series", "CurrentUserId", "dbo.Users");
            DropIndex("dbo.Drops", new[] { "DropId" });
            DropIndex("dbo.SimpleLines", new[] { "ContourId" });
            DropIndex("dbo.ReferencePhotos", new[] { "SimpleReferencePhotoLineId" });
            DropIndex("dbo.ReferencePhotos", new[] { "ReferencePhotoId" });
            DropIndex("dbo.Series", new[] { "CurrentUserId" });
            DropIndex("dbo.DropPhotos", new[] { "CurrentSeriesId" });
            DropIndex("dbo.DropPhotos", new[] { "SimpleVerticalLineId" });
            DropIndex("dbo.DropPhotos", new[] { "SimpleHorizontalLineId" });
            DropIndex("dbo.DbContours", new[] { "ContourId" });
            DropTable("dbo.DbLogEntries");
            DropTable("dbo.Drops");
            DropTable("dbo.SimpleLines");
            DropTable("dbo.ReferencePhotos");
            DropTable("dbo.Users");
            DropTable("dbo.Series");
            DropTable("dbo.DropPhotos");
            DropTable("dbo.DbContours");
        }
    }
}
