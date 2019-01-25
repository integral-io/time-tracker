using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TimeTracker.Data.Migrations
{
    public partial class timeentrytype : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TimeEntryTypeId",
                table: "TimeEntries",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ValueToCompany",
                table: "BillingClients",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TimeEntryType",
                columns: table => new
                {
                    TimeEntryTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsBillable = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEntryType", x => x.TimeEntryTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_TimeEntryTypeId",
                table: "TimeEntries",
                column: "TimeEntryTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_TimeEntryType_TimeEntryTypeId",
                table: "TimeEntries",
                column: "TimeEntryTypeId",
                principalTable: "TimeEntryType",
                principalColumn: "TimeEntryTypeId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_TimeEntryType_TimeEntryTypeId",
                table: "TimeEntries");

            migrationBuilder.DropTable(
                name: "TimeEntryType");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_TimeEntryTypeId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "TimeEntryTypeId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "ValueToCompany",
                table: "BillingClients");
        }
    }
}
