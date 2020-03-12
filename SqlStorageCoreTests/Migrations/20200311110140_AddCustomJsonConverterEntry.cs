using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SkbKontur.SqlStorageCore.Tests.Migrations
{
    public partial class AddCustomJsonConverterEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestCustomJsonConverterSqlEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CustomJsonColumn = table.Column<string>(type: "json", nullable: false, defaultValue: "null")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCustomJsonConverterSqlEntity", x => x.Id);
                })
                .Annotation("SkbKontur.SqlStorageCore:EventLogTrigger", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestCustomJsonConverterSqlEntity");
        }
    }
}
