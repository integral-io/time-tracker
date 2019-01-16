using Microsoft.EntityFrameworkCore.Migrations;

namespace TimeTracker.Data.Migrations
{
    public partial class clientProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "BillingClientId",
                table: "Projects",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BillingClientId",
                table: "Projects",
                column: "BillingClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_BillingClients_BillingClientId",
                table: "Projects",
                column: "BillingClientId",
                principalTable: "BillingClients",
                principalColumn: "BillingClientId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_BillingClients_BillingClientId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_BillingClientId",
                table: "Projects");

            migrationBuilder.AlterColumn<int>(
                name: "BillingClientId",
                table: "Projects",
                nullable: true,
                oldClrType: typeof(int));
        }
    }
}
