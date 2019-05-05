using Microsoft.EntityFrameworkCore.Migrations;

namespace TimeTracker.Data.Migrations
{
    public partial class removeclientfromtimeentry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_BillingClients_BillingClientId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_BillingClientId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "BillingClientId",
                table: "TimeEntries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingClientId",
                table: "TimeEntries",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_BillingClientId",
                table: "TimeEntries",
                column: "BillingClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_BillingClients_BillingClientId",
                table: "TimeEntries",
                column: "BillingClientId",
                principalTable: "BillingClients",
                principalColumn: "BillingClientId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
