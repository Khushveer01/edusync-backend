using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSync.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssessmentAndResultModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AttemptDate",
                table: "Results",
                newName: "CompletedAt");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Assessments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Assessments");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "Results",
                newName: "AttemptDate");
        }
    }
}
