using Microsoft.EntityFrameworkCore.Migrations;

namespace SkbKontur.SqlStorageCore.Tests.Migrations
{
    public partial class NullableReferenceTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", "'uuid-ossp', '', ''");

            migrationBuilder.AlterColumn<string>(
                name: "StringProperty",
                table: "TestValueTypedPropertiesStorageElement",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "TestTimestampElement",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ComplexColumn",
                table: "TestJsonColumnElement",
                type: "json",
                nullable: false,
                defaultValue: "null",
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true,
                oldDefaultValue: "null");

            migrationBuilder.AlterColumn<string>(
                name: "ComplexArrayColumn",
                table: "TestJsonArrayColumnElement",
                type: "json",
                nullable: false,
                defaultValue: "null",
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true,
                oldDefaultValue: "null");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "TestBatchStorageElement",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "SqlEventLogEntry",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModificationType",
                table: "SqlEventLogEntry",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "SqlEventLogEntry",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntityContent",
                table: "SqlEventLogEntry",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", "'uuid-ossp', '', ''")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "StringProperty",
                table: "TestValueTypedPropertiesStorageElement",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "TestTimestampElement",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<string>(
                name: "ComplexColumn",
                table: "TestJsonColumnElement",
                type: "json",
                nullable: true,
                defaultValue: "null",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValue: "null");

            migrationBuilder.AlterColumn<string>(
                name: "ComplexArrayColumn",
                table: "TestJsonArrayColumnElement",
                type: "json",
                nullable: true,
                defaultValue: "null",
                oldClrType: typeof(string),
                oldType: "json",
                oldDefaultValue: "null");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "TestBatchStorageElement",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "SqlEventLogEntry",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<string>(
                name: "ModificationType",
                table: "SqlEventLogEntry",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                table: "SqlEventLogEntry",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "EntityContent",
                table: "SqlEventLogEntry",
                type: "json",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "json");
        }
    }
}
