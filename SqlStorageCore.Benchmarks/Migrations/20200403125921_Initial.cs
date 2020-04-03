using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SkbKontur.SqlStorageCore.Benchmarks.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    DateOfBirth = table.Column<DateTime>(nullable: false),
                    PersonnelNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                })
                .Annotation("SkbKontur.SqlStorageCore:EventLogTrigger", true);

            migrationBuilder.CreateTable(
                name: "SqlEventLogEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    EntityType = table.Column<string>(nullable: false),
                    EntityContent = table.Column<string>(type: "json", nullable: false),
                    ModificationType = table.Column<string>(nullable: false),
                    Offset = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    TransactionId = table.Column<long>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlEventLogEntry", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SqlEventLogEntry_Offset",
                table: "SqlEventLogEntry",
                column: "Offset")
                .Annotation("Npgsql:IndexMethod", "brin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "SqlEventLogEntry");
        }
    }
}
