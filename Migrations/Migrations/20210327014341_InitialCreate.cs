using Microsoft.EntityFrameworkCore.Migrations;

namespace Migrations.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "primary_table",
                columns: table => new
                {
                    _id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    _name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    _desc = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table", x => x._id);
                });

            migrationBuilder.CreateTable(
                name: "player_data",
                columns: table => new
                {
                    _id = table.Column<long>(type: "BIGINT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    _steamid = table.Column<long>(type: "BIGINT", nullable: false),
                    _ihlmmr = table.Column<int>(type: "int", nullable: false),
                    _dotammr = table.Column<int>(type: "int", nullable: false),
                    _adjmmr = table.Column<int>(type: "int", nullable: false),
                    _gameswon = table.Column<int>(type: "int", nullable: false),
                    _gameslost = table.Column<int>(type: "int", nullable: false),
                    _status = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_table", x => x._id);
                });

            }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "primary_table");

            migrationBuilder.DropTable(
                name: "player_records");
        }
    }
}
