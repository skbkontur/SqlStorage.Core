using Microsoft.EntityFrameworkCore.Migrations;

namespace SkbKontur.SqlStorageCore.Tests.Migrations
{
    public partial class AlterSqlEventLogEntryAddTransactionId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TransactionId",
                table: "SqlEventLogEntry",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "SqlEventLogEntry");
        }
    }
}
