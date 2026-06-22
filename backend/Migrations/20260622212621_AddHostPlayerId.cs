using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhistOnline.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHostPlayerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HostPlayerId",
                table: "Games",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HostPlayerId",
                table: "Games");
        }
    }
}
