using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentGateway.Migrations
{
    /// <inheritdoc />
    public partial class CategoriesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionCategory",
                table: "Transaction",
                newName: "TransactionCategories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TransactionCategories",
                table: "Transaction",
                newName: "TransactionCategory");
        }
    }
}
