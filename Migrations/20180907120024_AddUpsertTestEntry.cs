using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.Migrations
{
    public partial class AddUpsertTestEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestUpsertSqlEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SomeId1 = table.Column<Guid>(nullable: false),
                    SomeId2 = table.Column<Guid>(nullable: false),
                    StringValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestUpsertSqlEntry", x => x.Id);
                })
                .Annotation("EDI:EventLogTrigger", true);

            migrationBuilder.CreateIndex(
                name: "IX_TestUpsertSqlEntry_SomeId1_SomeId2",
                table: "TestUpsertSqlEntry",
                columns: new[] { "SomeId1", "SomeId2" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestUpsertSqlEntry");
        }
    }
}
