﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.DataStore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "LastLogon",
                table: "Users",
                newName: "LastPasswordChangedDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogonDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogonDate",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "LastPasswordChangedDate",
                table: "Users",
                newName: "LastLogon");

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "Users",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
