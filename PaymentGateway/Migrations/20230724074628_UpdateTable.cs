using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sender_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender_ID_NO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender_Phone_No = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sender_Src_Account = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Receiver_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Receiver_ID_NO = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Receiver_Phone_No = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Receiver_Dst_Account = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transaction");
        }
    }
}
