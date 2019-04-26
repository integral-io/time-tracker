using Microsoft.EntityFrameworkCore.Migrations;

namespace TimeTracker.Data.Migrations
{
    public partial class addGoogleStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleIdentifier",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationEmail",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleIdentifier",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationEmail",
                table: "Users");
        }
    }
}
