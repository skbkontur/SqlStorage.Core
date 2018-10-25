using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.Migrations
{
    public partial class InitialCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", "'uuid-ossp', '', ''");

            migrationBuilder.CreateTable(
                name: "SqlEventLogEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    EntityType = table.Column<string>(nullable: true),
                    EntityContent = table.Column<string>(nullable: true),
                    ModificationType = table.Column<string>(nullable: true),
                    Offset = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Timestamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SqlEventLogEntry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestJsonArrayColumnElement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ComplexArrayColumn = table.Column<string>(type: "json", nullable: true, defaultValue: "null")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestJsonArrayColumnElement", x => x.Id);
                })
                .Annotation("EDI:EventLogTrigger", true);

            migrationBuilder.CreateTable(
                name: "TestJsonColumnElement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ComplexColumn = table.Column<string>(type: "json", nullable: true, defaultValue: "null")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestJsonColumnElement", x => x.Id);
                })
                .Annotation("EDI:EventLogTrigger", true);

            migrationBuilder.CreateTable(
                name: "TestTimestampElement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Timestamp = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTimestampElement", x => x.Id);
                })
                .Annotation("EDI:EventLogTrigger", true);

            migrationBuilder.CreateTable(
                name: "TestValueTypedPropertiesStorageElement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StringProperty = table.Column<string>(nullable: true),
                    IntProperty = table.Column<int>(nullable: true),
                    BoolProperty = table.Column<bool>(nullable: true),
                    DateTimeProperty = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestValueTypedPropertiesStorageElement", x => x.Id);
                })
                .Annotation("EDI:EventLogTrigger", true);

            migrationBuilder.CreateIndex(
                name: "IX_SqlEventLogEntry_Offset",
                table: "SqlEventLogEntry",
                column: "Offset")
                .Annotation("Npgsql:IndexMethod", "brin");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SqlEventLogEntry");

            migrationBuilder.DropTable(
                name: "TestJsonArrayColumnElement");

            migrationBuilder.DropTable(
                name: "TestJsonColumnElement");

            migrationBuilder.DropTable(
                name: "TestTimestampElement");

            migrationBuilder.DropTable(
                name: "TestValueTypedPropertiesStorageElement");
        }
    }
}
