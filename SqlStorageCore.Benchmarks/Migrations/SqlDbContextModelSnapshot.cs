﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SkbKontur.SqlStorageCore;

namespace SkbKontur.SqlStorageCore.Benchmarks.Migrations
{
    [DbContext(typeof(SqlDbContext))]
    partial class SqlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("SkbKontur.SqlStorageCore.Benchmarks.Entries.Employee", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PersonnelNumber")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Employee");

                    b.HasAnnotation("SkbKontur.SqlStorageCore:EventLogTrigger", true);
                });

            modelBuilder.Entity("SkbKontur.SqlStorageCore.EventLog.SqlEventLogEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<string>("EntityContent")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ModificationType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Offset")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<long>("TransactionId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("Offset")
                        .HasAnnotation("Npgsql:IndexMethod", "brin");

                    b.ToTable("SqlEventLogEntry");
                });
#pragma warning restore 612, 618
        }
    }
}
