using System;

using Microsoft.EntityFrameworkCore.Migrations;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.Migrations
{
    public partial class EventLogEntryTimestampLong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name : "Timestamp",
                table : "SqlEventLogEntry");

            migrationBuilder.AddColumn<long>(
                name : "Timestamp",
                table : "SqlEventLogEntry",
                nullable : true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name : "Timestamp",
                table : "SqlEventLogEntry");

            migrationBuilder.AddColumn<DateTime>(
                name : "Timestamp",
                table : "SqlEventLogEntry",
                nullable : false);
        }
    }
}