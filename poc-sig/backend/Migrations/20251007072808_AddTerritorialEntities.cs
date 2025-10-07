using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PocSig.Migrations
{
    /// <inheritdoc />
    public partial class AddTerritorialEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Communes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeInsee = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Population = table.Column<int>(type: "int", nullable: true),
                    Geometry = table.Column<Point>(type: "geography", nullable: false),
                    DepartementCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DepartementNom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EPCICode = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    EPCINom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departements",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeDept = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Population = table.Column<int>(type: "int", nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    RegionNom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EPCIs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodeSiren = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeEPCI = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DepartementCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    CommunesCount = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EPCIs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Communes_CodeInsee",
                schema: "dbo",
                table: "Communes",
                column: "CodeInsee",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Communes_DepartementCode",
                schema: "dbo",
                table: "Communes",
                column: "DepartementCode");

            migrationBuilder.CreateIndex(
                name: "IX_Communes_Nom",
                schema: "dbo",
                table: "Communes",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_Departements_CodeDept",
                schema: "dbo",
                table: "Departements",
                column: "CodeDept",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departements_Nom",
                schema: "dbo",
                table: "Departements",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_EPCIs_CodeSiren",
                schema: "dbo",
                table: "EPCIs",
                column: "CodeSiren",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EPCIs_Nom",
                schema: "dbo",
                table: "EPCIs",
                column: "Nom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Communes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Departements",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "EPCIs",
                schema: "dbo");
        }
    }
}
