using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Selfix.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    image_generations_count = table.Column<int>(type: "integer", nullable: false),
                    avatar_generations_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_packages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "prompts",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    number_in_order = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "varchar(8192)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "varchar(128)", nullable: false),
                    type = table.Column<string>(type: "varchar(32)", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    package_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.ForeignKey(
                        name: "FK_products_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "avatars",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "varchar(64)", nullable: false),
                    description = table.Column<string>(type: "varchar(8192)", nullable: false),
                    os_lora_file_path = table.Column<string>(type: "varchar(256)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avatars", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    avatar_generations_count = table.Column<int>(type: "integer", nullable: false),
                    image_generations_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    invited_by_id = table.Column<string>(type: "text", nullable: true),
                    active_avatar_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_avatars_active_avatar_id",
                        column: x => x.active_avatar_id,
                        principalTable: "avatars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_users_invited_by_id",
                        column: x => x.invited_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    os_file_path = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_images_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "varchar(32)", nullable: false),
                    status = table.Column<string>(type: "varchar(32)", nullable: false),
                    input = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    output = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_jobs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "promocodes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "varchar(32)", nullable: false),
                    used_by_user_id = table.Column<string>(type: "text", nullable: true),
                    ProductId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promocodes", x => x.id);
                    table.ForeignKey(
                        name: "FK_promocodes_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_promocodes_users_used_by_user_id",
                        column: x => x.used_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "telegram_profiles",
                columns: table => new
                {
                    telegram_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.None),
                    chat_state = table.Column<string>(type: "varchar(32)", nullable: false),
                    settings = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    state_data = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telegram_profiles", x => x.telegram_id);
                    table.ForeignKey(
                        name: "FK_telegram_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "varchar(16)", nullable: false),
                    type = table.Column<string>(type: "varchar(16)", nullable: false),
                    payment_data = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    product_id = table.Column<string>(type: "text", nullable: true),
                    promocode_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_orders_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_orders_promocodes_promocode_id",
                        column: x => x.promocode_id,
                        principalTable: "promocodes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "packages",
                columns: new[] { "id", "avatar_generations_count", "image_generations_count" },
                values: new object[,]
                {
                    { "00S2G6N181WCX6781J4X8A98XX", 1, 0 },
                    { "00S2G6N181WDQQRE4RPZEJ02Y1", 0, 100 },
                    { "00S2G6N181WJHSBKZZ8SMSRWY4", 1, 20 },
                    { "00S2G6N181WKCATSV5TQTHKPY8", 1, 100 }
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "id", "discount", "is_active", "name", "package_id", "price", "type" },
                values: new object[,]
                {
                    { "00S2G6N181NJWTPSA06P0G9SBC", 1000m, true, "1 аватар", "00S2G6N181WCX6781J4X8A98XX", 499m, "Package" },
                    { "00S2G6N181NKPW9Z56RM6R0KBF", 1000m, true, "100 генераций", "00S2G6N181WDQQRE4RPZEJ02Y1", 999m, "Package" },
                    { "00S2G6N181NMHDV50CAJDFVDBK", 1000m, true, "Специальное предложение (1 аватар, 20 генераций)", "00S2G6N181WJHSBKZZ8SMSRWY4", 399m, "TrialPackage" },
                    { "00S2G6N181NNBFCB3JWCK7J7BP", 1000m, true, "Начальный пакет (1 аватар, 100 генераций)", "00S2G6N181WKCATSV5TQTHKPY8", 999m, "FirstPaymentPackage" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_avatars_user_id",
                table: "avatars",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_images_user_id",
                table: "images",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_user_id",
                table: "jobs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_product_id",
                table: "orders",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_promocode_id",
                table: "orders",
                column: "promocode_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_package_id",
                table: "products",
                column: "package_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promocodes_code",
                table: "promocodes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promocodes_ProductId",
                table: "promocodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_promocodes_used_by_user_id",
                table: "promocodes",
                column: "used_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_telegram_profiles_user_id",
                table: "telegram_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_active_avatar_id",
                table: "users",
                column: "active_avatar_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_invited_by_id",
                table: "users",
                column: "invited_by_id");

            migrationBuilder.AddForeignKey(
                name: "FK_avatars_users_user_id",
                table: "avatars",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_avatars_users_user_id",
                table: "avatars");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "prompts");

            migrationBuilder.DropTable(
                name: "telegram_profiles");

            migrationBuilder.DropTable(
                name: "promocodes");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "avatars");
        }
    }
}
