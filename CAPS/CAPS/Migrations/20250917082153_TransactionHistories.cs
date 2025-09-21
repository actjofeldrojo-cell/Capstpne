using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPS.Migrations
{
    /// <inheritdoc />
    public partial class TransactionHistories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rooms");

            migrationBuilder.CreateTable(
                name: "TransactionHistories",
                columns: table => new
                {
                    TransactionHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientGender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientAge = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientDateRegistered = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AvailedServices = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TenderedAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ChangeAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: true),
                    StaffName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StaffExpertise = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    RoomNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RoomType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionHistories", x => x.TransactionHistoryId);
                    table.ForeignKey(
                        name: "FK_TransactionHistories_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionHistories_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK_TransactionHistories_Staffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staffs",
                        principalColumn: "StaffId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_ClientId",
                table: "TransactionHistories",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_RoomId",
                table: "TransactionHistories",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionHistories_StaffId",
                table: "TransactionHistories",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionHistories");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rooms",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
