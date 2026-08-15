using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nearbly.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStorePublicCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "public_code",
                table: "stores",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE stores
                SET public_code = 's_' || REPLACE(id::text, '-', '')
                WHERE public_code IS NULL;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "public_code",
                table: "stores",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_stores_public_code",
                table: "stores",
                column: "public_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_stores_public_code",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "public_code",
                table: "stores");
        }
    }
}
