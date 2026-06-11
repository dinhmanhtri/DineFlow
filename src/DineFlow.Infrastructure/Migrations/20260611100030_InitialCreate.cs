using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DineFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiningTables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableNumber = table.Column<int>(type: "int", nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiningTables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staffs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_DiningTables_TableId",
                        column: x => x.TableId,
                        principalTable: "DiningTables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CashierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Staffs_CashierId",
                        column: x => x.CashierId,
                        principalTable: "Staffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_MenuItems_MenuItemId",
                        column: x => x.MenuItemId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "DisplayOrder", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, "Khai vị", null },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, true, "Món chính", null },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, "Tráng miệng", null },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 4, true, "Đồ uống", null }
                });

            migrationBuilder.InsertData(
                table: "DiningTables",
                columns: new[] { "Id", "Capacity", "CreatedAt", "FloorNumber", "Status", "TableNumber", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("33333333-0000-0000-0000-000000000001"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Available", 1, null },
                    { new Guid("33333333-0000-0000-0000-000000000002"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Available", 2, null },
                    { new Guid("33333333-0000-0000-0000-000000000003"), 6, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Available", 3, null },
                    { new Guid("33333333-0000-0000-0000-000000000004"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Available", 4, null },
                    { new Guid("33333333-0000-0000-0000-000000000005"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 1, "Available", 5, null },
                    { new Guid("33333333-0000-0000-0000-000000000006"), 6, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Available", 6, null },
                    { new Guid("33333333-0000-0000-0000-000000000007"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Available", 7, null },
                    { new Guid("33333333-0000-0000-0000-000000000008"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Available", 8, null },
                    { new Guid("33333333-0000-0000-0000-000000000009"), 6, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Available", 9, null },
                    { new Guid("33333333-0000-0000-0000-000000000010"), 4, new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), 2, "Available", 10, null }
                });

            migrationBuilder.InsertData(
                table: "Staffs",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[] { new Guid("44444444-0000-0000-0000-000000000001"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), "admin@dineflow.com", "System Admin", true, "$2a$11$qRzN7p9G5R34K.QYvS.l4.Qh474X2tO751853n2y90E9/wD54992y", null, "Admin", null });

            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "ImageUrl", "IsAvailable", "Name", "Price", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Gỏi cuốn tôm thịt", 45000m, null },
                    { new Guid("22222222-0000-0000-0000-000000000002"), new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Bò lúc lắc", 185000m, null },
                    { new Guid("22222222-0000-0000-0000-000000000003"), new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Cơm tấm sườn bì", 65000m, null },
                    { new Guid("22222222-0000-0000-0000-000000000004"), new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Chè ba màu", 35000m, null },
                    { new Guid("22222222-0000-0000-0000-000000000005"), new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Nước ép cam", 30000m, null },
                    { new Guid("22222222-0000-0000-0000-000000000006"), new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2026, 6, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, "Trà đá", 10000m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_DisplayOrder",
                table: "Categories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_DiningTables_TableNumber",
                table: "DiningTables",
                column: "TableNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CashierId",
                table: "Invoices",
                column: "CashierId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                table: "Invoices",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CategoryId",
                table: "MenuItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MenuItemId",
                table: "OrderItems",
                column: "MenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StaffId",
                table: "Orders",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TableId",
                table: "Orders",
                column: "TableId");

            migrationBuilder.CreateIndex(
                name: "IX_Staffs_Email",
                table: "Staffs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "DiningTables");

            migrationBuilder.DropTable(
                name: "Staffs");
        }
    }
}
