using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PocSig.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Layers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Srid = table.Column<int>(type: "int", nullable: false, defaultValue: 4326),
                    GeometryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LayerId = table.Column<int>(type: "int", nullable: false),
                    PropertiesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Geometry = table.Column<Geometry>(type: "geometry", nullable: false),
                    ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ValidToUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_Layers_LayerId",
                        column: x => x.LayerId,
                        principalSchema: "dbo",
                        principalTable: "Layers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Features_LayerId",
                schema: "dbo",
                table: "Features",
                column: "LayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_ValidFromUtc",
                schema: "dbo",
                table: "Features",
                column: "ValidFromUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Features_ValidToUtc",
                schema: "dbo",
                table: "Features",
                column: "ValidToUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Layers_Name",
                schema: "dbo",
                table: "Layers",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Features",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Layers",
                schema: "dbo");
        }
    }
}
