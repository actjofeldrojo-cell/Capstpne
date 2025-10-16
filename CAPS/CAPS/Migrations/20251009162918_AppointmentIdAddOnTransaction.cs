using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPS.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentIdAddOnTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AppointmentId",
                table: "Transactions",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Appointments_AppointmentId",
                table: "Transactions",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "AppointmentId",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Appointments_AppointmentId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_AppointmentId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Transactions");
        }
    }
}
