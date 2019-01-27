﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SKBKontur.Catalogue.EDI.SqlStorageCore;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.Migrations
{
    [DbContext(typeof(SqlDbContext))]
    partial class SqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:PostgresExtension:uuid-ossp", "'uuid-ossp', '', ''")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.1.4-rtm-31024")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog.SqlEventLogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<string>("EntityContent")
                        .HasColumnType("json");

                    b.Property<string>("EntityType");

                    b.Property<string>("ModificationType");

                    b.Property<long>("Offset")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<long?>("Timestamp");

                    b.HasKey("Id");

                    b.HasIndex("Offset")
                        .HasAnnotation("Npgsql:IndexMethod", "brin");

                    b.ToTable("SqlEventLogEntry");
                });

            modelBuilder.Entity("SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities.TestBatchStorageElement", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("Value");

                    b.HasKey("Id");

                    b.ToTable("TestBatchStorageElement");

                    b.HasAnnotation("EDI:EventLogTrigger", true);
                });

            modelBuilder.Entity("SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities.TestJsonArrayColumnElement", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("ComplexArrayColumn")
                        .HasColumnType("json")
                        .HasDefaultValue("null");

                    b.HasKey("Id");

                    b.ToTable("TestJsonArrayColumnElement");

                    b.HasAnnotation("EDI:EventLogTrigger", true);
                });

            modelBuilder.Entity("SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities.TestJsonColumnElement", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("ComplexColumn")
                        .HasColumnType("json")
                        .HasDefaultValue("null");

                    b.HasKey("Id");

                    b.ToTable("TestJsonColumnElement");

                    b.HasAnnotation("EDI:EventLogTrigger", true);
                });

            modelBuilder.Entity("SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities.TestTimestampElement", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<long?>("Timestamp");

                    b.HasKey("Id");

                    b.ToTable("TestTimestampElement");

                    b.HasAnnotation("EDI:EventLogTrigger", true);
                });

            modelBuilder.Entity("SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities.TestUpsertSqlEntry", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<string>("RequiredValue")
                        .IsRequired();

                    b.Property<Guid>("SomeId1");

                    b.Property<Guid>("SomeId2");

                    b.Property<string>("StringValue");

                    b.HasKey("Id");

                    b.HasIndex("SomeId1", "SomeId2")
                        .IsUnique();

                    b.ToTable("TestUpsertSqlEntry");

                    b.HasAnnotation("EDI:EventLogTrigger", true);
                });

            modelBuilder.Entity("SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities.TestValueTypedPropertiesStorageElement", b =>
                {
                    b.Property<Guid>("Id");

                    b.Property<bool?>("BoolProperty");

                    b.Property<DateTime?>("DateTimeProperty");

                    b.Property<int?>("IntProperty");

                    b.Property<string>("StringProperty");

                    b.HasKey("Id");

                    b.ToTable("TestValueTypedPropertiesStorageElement");

                    b.HasAnnotation("EDI:EventLogTrigger", true);
                });
#pragma warning restore 612, 618
        }
    }
}