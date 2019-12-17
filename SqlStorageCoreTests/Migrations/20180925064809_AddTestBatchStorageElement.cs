using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace SkbKontur.SqlStorageCore.Tests.Migrations
{
    public partial class AddTestBatchStorageElement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name : "EntityContent",
                table : "SqlEventLogEntry");

            migrationBuilder.AddColumn<string>(
                name : "EntityContent",
                table : "SqlEventLogEntry",
                type : "json",
                nullable : true);

            migrationBuilder.CreateTable(
                name : "TestBatchStorageElement",
                columns : table => new
                    {
                        Id = table.Column<Guid>(nullable : false),
                        Value = table.Column<string>(nullable : true)
                    },
                constraints : table => { table.PrimaryKey("PK_TestBatchStorageElement", x => x.Id); })
                            .Annotation("SkbKontur.SqlStorageCore:EventLogTrigger", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name : "TestBatchStorageElement");

            migrationBuilder.DropColumn(
                name : "EntityContent",
                table : "SqlEventLogEntry");

            migrationBuilder.AddColumn<string>(
                name : "EntityContent",
                table : "SqlEventLogEntry",
                nullable : true);
        }
    }
}