using Microsoft.EntityFrameworkCore.Migrations;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.Migrations
{
    public partial class AddRequiredFieldToTestUpsertSqlEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredValue",
                table: "TestUpsertSqlEntry",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredValue",
                table: "TestUpsertSqlEntry");
        }
    }
}
