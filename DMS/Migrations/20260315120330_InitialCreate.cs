using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Użytkownicy",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Użytkownicy", x => x.UUID);
                });

            migrationBuilder.CreateTable(
                name: "Wiadomości_Prywatne",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SenderUUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReceiverUUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wiadomości_Prywatne", x => x.MessageId);
                    table.CheckConstraint("CK_Message_NotEmpty", "trim(Content) <> ''");
                });

            migrationBuilder.CreateTable(
                name: "Info_Użytkownicy",
                columns: table => new
                {
                    UUID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayMode = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Info_Użytkownicy", x => x.UUID);
                    table.ForeignKey(
                        name: "FK_Info_Użytkownicy_Użytkownicy_UUID",
                        column: x => x.UUID,
                        principalTable: "Użytkownicy",
                        principalColumn: "UUID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Info_Użytkownicy");

            migrationBuilder.DropTable(
                name: "Wiadomości_Prywatne");

            migrationBuilder.DropTable(
                name: "Użytkownicy");
        }
    }
}
