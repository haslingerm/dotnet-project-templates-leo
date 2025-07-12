using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ninjas.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ninjas");

            migrationBuilder.CreateTable(
                name: "mission",
                schema: "ninjas",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    dangerousness = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mission", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ninja",
                schema: "ninjas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code_name = table.Column<string>(type: "text", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    current_mission_id = table.Column<long>(type: "bigint", nullable: true),
                    weapon_proficiencies = table.Column<int[]>(type: "integer[]", nullable: false),
                    special_skills = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ninja", x => x.id);
                    table.ForeignKey(
                        name: "fk_ninja_mission_current_mission_id",
                        column: x => x.current_mission_id,
                        principalSchema: "ninjas",
                        principalTable: "mission",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ninja_current_mission_id",
                schema: "ninjas",
                table: "ninja",
                column: "current_mission_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ninja",
                schema: "ninjas");

            migrationBuilder.DropTable(
                name: "mission",
                schema: "ninjas");
        }
    }
}
