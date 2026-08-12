using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nearbly.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypesAndMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "logo_media_id",
                table: "stores",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "store_tabs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Links");

            migrationBuilder.Sql("""
                INSERT INTO store_tabs (id, store_id, key, name, content_type, sort_order, is_active, created_at_utc, updated_at_utc)
                SELECT gen_random_uuid(), s.id, 'links', 'Links', 'Links', COALESCE(MAX(existing.sort_order) + 1, 0), TRUE, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP
                FROM stores s
                INNER JOIN links root_link ON root_link.store_id = s.id AND root_link.store_tab_id IS NULL
                LEFT JOIN store_tabs existing ON existing.store_id = s.id
                WHERE NOT EXISTS (SELECT 1 FROM store_tabs named_links WHERE named_links.store_id = s.id AND named_links.key = 'links')
                GROUP BY s.id;

                UPDATE links root_link
                SET store_tab_id = links_tab.id
                FROM store_tabs links_tab
                WHERE root_link.store_tab_id IS NULL
                  AND links_tab.store_id = root_link.store_id
                  AND links_tab.key = 'links';
                """);

            migrationBuilder.CreateTable(
                name: "markdown_blocks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_tab_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    markdown = table.Column<string>(type: "character varying(20000)", maxLength: 20000, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_markdown_blocks", x => x.id);
                    table.ForeignKey(
                        name: "fk_markdown_blocks_store_tabs_store_tab_id",
                        column: x => x.store_tab_id,
                        principalTable: "store_tabs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_markdown_blocks_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "media_assets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media_assets", x => x.id);
                    table.ForeignKey(
                        name: "fk_media_assets_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gallery_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_tab_id = table.Column<Guid>(type: "uuid", nullable: false),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alt_text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gallery_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_gallery_items_media_assets_media_asset_id",
                        column: x => x.media_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gallery_items_store_tabs_store_tab_id",
                        column: x => x.store_tab_id,
                        principalTable: "store_tabs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_gallery_items_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: false),
                    store_tab_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    media_asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_products_media_assets_media_asset_id",
                        column: x => x.media_asset_id,
                        principalTable: "media_assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_store_tabs_store_tab_id",
                        column: x => x.store_tab_id,
                        principalTable: "store_tabs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_products_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stores_logo_media_id",
                table: "stores",
                column: "logo_media_id");

            migrationBuilder.CreateIndex(
                name: "ix_gallery_items_media_asset_id",
                table: "gallery_items",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_gallery_items_store_id",
                table: "gallery_items",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_gallery_items_store_tab_id_is_active_sort_order",
                table: "gallery_items",
                columns: new[] { "store_tab_id", "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_markdown_blocks_store_id",
                table: "markdown_blocks",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_markdown_blocks_store_tab_id_is_active_sort_order",
                table: "markdown_blocks",
                columns: new[] { "store_tab_id", "is_active", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_media_assets_store_id_is_active_created_at_utc",
                table: "media_assets",
                columns: new[] { "store_id", "is_active", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_products_media_asset_id",
                table: "products",
                column: "media_asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_store_id",
                table: "products",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_store_tab_id_is_active_sort_order",
                table: "products",
                columns: new[] { "store_tab_id", "is_active", "sort_order" });

            migrationBuilder.AddForeignKey(
                name: "fk_stores_media_assets_logo_media_id",
                table: "stores",
                column: "logo_media_id",
                principalTable: "media_assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_stores_media_assets_logo_media_id",
                table: "stores");

            migrationBuilder.DropTable(
                name: "gallery_items");

            migrationBuilder.DropTable(
                name: "markdown_blocks");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "media_assets");

            migrationBuilder.DropIndex(
                name: "ix_stores_logo_media_id",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "logo_media_id",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "store_tabs");
        }
    }
}
