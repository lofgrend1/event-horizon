using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HF.EventHorizon.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProtocolPlugins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PluginDirectoryPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PluginTypesCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequiredParametersCsv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolPlugins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProtocolPluginId = table.Column<int>(type: "int", nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolConnections_ProtocolPlugins_ProtocolPluginId",
                        column: x => x.ProtocolPluginId,
                        principalTable: "ProtocolPlugins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BrowsedAddresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolConnectionId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrowsedAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrowsedAddresses_ProtocolConnections_ProtocolConnectionId",
                        column: x => x.ProtocolConnectionId,
                        principalTable: "ProtocolConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolConnectionId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutingRules_ProtocolConnections_ProtocolConnectionId",
                        column: x => x.ProtocolConnectionId,
                        principalTable: "ProtocolConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DestinationMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolConnectionId = table.Column<int>(type: "int", nullable: false),
                    RoutingRuleId = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationMaps_ProtocolConnections_ProtocolConnectionId",
                        column: x => x.ProtocolConnectionId,
                        principalTable: "ProtocolConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DestinationMaps_RoutingRules_RoutingRuleId",
                        column: x => x.RoutingRuleId,
                        principalTable: "RoutingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrowsedAddresses_ProtocolConnectionId",
                table: "BrowsedAddresses",
                column: "ProtocolConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationMaps_ProtocolConnectionId",
                table: "DestinationMaps",
                column: "ProtocolConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DestinationMaps_RoutingRuleId",
                table: "DestinationMaps",
                column: "RoutingRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolConnections_Name",
                table: "ProtocolConnections",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolConnections_ProtocolPluginId",
                table: "ProtocolConnections",
                column: "ProtocolPluginId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutingRules_ProtocolConnectionId",
                table: "RoutingRules",
                column: "ProtocolConnectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrowsedAddresses");

            migrationBuilder.DropTable(
                name: "DestinationMaps");

            migrationBuilder.DropTable(
                name: "RoutingRules");

            migrationBuilder.DropTable(
                name: "ProtocolConnections");

            migrationBuilder.DropTable(
                name: "ProtocolPlugins");
        }
    }
}
