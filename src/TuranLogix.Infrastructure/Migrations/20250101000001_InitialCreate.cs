using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TuranLogix.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CompanyName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                Bin = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                IsPhoneVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                TelegramChatId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Orders",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                OrderNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                ClientId = table.Column<int>(type: "integer", nullable: false),
                OriginCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                DestinationCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                OriginLat = table.Column<double>(type: "double precision", nullable: true),
                OriginLng = table.Column<double>(type: "double precision", nullable: true),
                DestinationLat = table.Column<double>(type: "double precision", nullable: true),
                DestinationLng = table.Column<double>(type: "double precision", nullable: true),
                CargoDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Weight = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                Volume = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                CargoType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                PickupDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Orders", x => x.Id);
                table.ForeignKey(
                    name: "FK_Orders_Users_ClientId",
                    column: x => x.ClientId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                FileHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                OrderId = table.Column<int>(type: "integer", nullable: false),
                UploadedByUserId = table.Column<int>(type: "integer", nullable: false),
                IsSigned = table.Column<bool>(type: "boolean", nullable: false),
                SignatureData = table.Column<string>(type: "text", nullable: true),
                SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Documents", x => x.Id);
                table.ForeignKey(
                    name: "FK_Documents_Orders_OrderId",
                    column: x => x.OrderId,
                    principalTable: "Orders",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChatMessages",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                SessionId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<int>(type: "integer", nullable: true),
                Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatMessages", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_OrderNumber",
            table: "Orders",
            column: "OrderNumber",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Orders_ClientId",
            table: "Orders",
            column: "ClientId");

        migrationBuilder.CreateIndex(
            name: "IX_Documents_OrderId",
            table: "Documents",
            column: "OrderId");

        migrationBuilder.CreateIndex(
            name: "IX_ChatMessages_SessionId",
            table: "ChatMessages",
            column: "SessionId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ChatMessages");
        migrationBuilder.DropTable(name: "Documents");
        migrationBuilder.DropTable(name: "Orders");
        migrationBuilder.DropTable(name: "Users");
    }
}
