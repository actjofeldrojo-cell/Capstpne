using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPS.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "ComfortItemPreferences",
                table: "Clients",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MassagePressureLevel",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MusicPreference",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredTherapistGender",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemperaturePreference",
                table: "Clients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComfortItemPreferences",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MassagePressureLevel",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "MusicPreference",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "PreferredTherapistGender",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "TemperaturePreference",
                table: "Clients");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Clients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
