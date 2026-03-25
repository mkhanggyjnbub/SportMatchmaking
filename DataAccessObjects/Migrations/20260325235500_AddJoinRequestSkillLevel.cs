using System;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccessObjects.Migrations
{
    public partial class AddJoinRequestSkillLevel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "SkillLevel",
                table: "JoinRequests",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillLevel",
                table: "JoinRequests");
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.23")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("BusinessObjects.JoinRequest", b =>
                {
                    b.Property<long>("RequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("RequestId"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasDefaultValueSql("(sysutcdatetime())");

                    b.Property<DateTime?>("DecidedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)");

                    b.Property<int?>("DecidedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("GuestNames")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<string>("Message")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("PartySize")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<long>("PostId")
                        .HasColumnType("bigint");

                    b.Property<int>("RequesterUserId")
                        .HasColumnType("int");

                    b.Property<byte>("SkillLevel")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint")
                        .HasDefaultValue((byte)1);

                    b.Property<byte>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint")
                        .HasDefaultValue((byte)1);

                    b.HasKey("RequestId");

                    b.HasIndex("DecidedByUserId");

                    b.HasIndex("RequesterUserId");

                    b.HasIndex(new[] { "PostId", "Status", "CreatedAt" }, "IX_JoinRequests_Post_Status");

                    b.HasIndex(new[] { "PostId", "RequesterUserId" }, "UX_JoinRequests_Unique")
                        .IsUnique();

                    b.ToTable("JoinRequests");
                });
        }
    }
}
