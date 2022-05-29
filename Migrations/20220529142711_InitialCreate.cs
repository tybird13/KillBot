using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KillBot.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MyProperty = table.Column<int>(type: "INTEGER", nullable: false),
                    KillerUsername = table.Column<string>(type: "TEXT", nullable: false),
                    TargetUsername = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kills", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kills");
        }
    }
}
