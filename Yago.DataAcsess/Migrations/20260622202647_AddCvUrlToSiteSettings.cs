using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yago.DataAcsess.Migrations
{
    /// <inheritdoc />
    public partial class AddCvUrlToSiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CvUrl",
                table: "SiteSettings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CvUrl",
                table: "SiteSettings");
        }
    }
}
