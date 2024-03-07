using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations
{
	/// <inheritdoc />
	public partial class AddGroupMessageAndNotifications : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "RecipientDeleted",
					table: "Messages");

			migrationBuilder.DropColumn(
					name: "SenderDeleted",
					table: "Messages");

			migrationBuilder.RenameColumn(
					name: "MessageSent",
					table: "Messages",
					newName: "CreatedAt");

			migrationBuilder.RenameColumn(
					name: "Created",
					table: "AspNetUsers",
					newName: "MemberSince");

			migrationBuilder.CreateTable(
					name: "GroupMessages",
					columns: table => new
					{
						Id = table.Column<int>(type: "INTEGER", nullable: false)
									.Annotation("Sqlite:Autoincrement", true),
						ChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
						ChannelName = table.Column<string>(type: "TEXT", nullable: true),
						SenderId = table.Column<int>(type: "INTEGER", nullable: false),
						Content = table.Column<string>(type: "TEXT", nullable: true),
						CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_GroupMessages", x => x.Id);
						table.ForeignKey(
											name: "FK_GroupMessages_AspNetUsers_SenderId",
											column: x => x.SenderId,
											principalTable: "AspNetUsers",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "AppUserGroupMessage",
					columns: table => new
					{
						GroupMessagesId = table.Column<int>(type: "INTEGER", nullable: false),
						UsersId = table.Column<int>(type: "INTEGER", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_AppUserGroupMessage", x => new { x.GroupMessagesId, x.UsersId });
						table.ForeignKey(
											name: "FK_AppUserGroupMessage_AspNetUsers_UsersId",
											column: x => x.UsersId,
											principalTable: "AspNetUsers",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_AppUserGroupMessage_GroupMessages_GroupMessagesId",
											column: x => x.GroupMessagesId,
											principalTable: "GroupMessages",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "Notifications",
					columns: table => new
					{
						Id = table.Column<int>(type: "INTEGER", nullable: false)
									.Annotation("Sqlite:Autoincrement", true),
						SenderId = table.Column<int>(type: "INTEGER", nullable: false),
						RecipientId = table.Column<int>(type: "INTEGER", nullable: true),
						MessageId = table.Column<int>(type: "INTEGER", nullable: true),
						GroupMessageId = table.Column<int>(type: "INTEGER", nullable: true)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Notifications", x => x.Id);
						table.ForeignKey(
											name: "FK_Notifications_AspNetUsers_RecipientId",
											column: x => x.RecipientId,
											principalTable: "AspNetUsers",
											principalColumn: "Id",
											onDelete: ReferentialAction.Restrict);
						table.ForeignKey(
											name: "FK_Notifications_AspNetUsers_SenderId",
											column: x => x.SenderId,
											principalTable: "AspNetUsers",
											principalColumn: "Id",
											onDelete: ReferentialAction.Restrict);
						table.ForeignKey(
											name: "FK_Notifications_GroupMessages_GroupMessageId",
											column: x => x.GroupMessageId,
											principalTable: "GroupMessages",
											principalColumn: "Id",
											onDelete: ReferentialAction.Restrict);
						table.ForeignKey(
											name: "FK_Notifications_Messages_MessageId",
											column: x => x.MessageId,
											principalTable: "Messages",
											principalColumn: "Id",
											onDelete: ReferentialAction.Restrict);
					});

			migrationBuilder.CreateIndex(
					name: "IX_AppUserGroupMessage_UsersId",
					table: "AppUserGroupMessage",
					column: "UsersId");

			migrationBuilder.CreateIndex(
					name: "IX_GroupMessages_SenderId",
					table: "GroupMessages",
					column: "SenderId");

			migrationBuilder.CreateIndex(
					name: "IX_Notifications_GroupMessageId",
					table: "Notifications",
					column: "GroupMessageId");

			migrationBuilder.CreateIndex(
					name: "IX_Notifications_MessageId",
					table: "Notifications",
					column: "MessageId");

			migrationBuilder.CreateIndex(
					name: "IX_Notifications_RecipientId",
					table: "Notifications",
					column: "RecipientId");

			migrationBuilder.CreateIndex(
					name: "IX_Notifications_SenderId",
					table: "Notifications",
					column: "SenderId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "AppUserGroupMessage");

			migrationBuilder.DropTable(
					name: "Notifications");

			migrationBuilder.DropTable(
					name: "GroupMessages");

			migrationBuilder.RenameColumn(
					name: "CreatedAt",
					table: "Messages",
					newName: "MessageSent");

			migrationBuilder.RenameColumn(
					name: "MemberSince",
					table: "AspNetUsers",
					newName: "Created");

			migrationBuilder.AddColumn<bool>(
					name: "RecipientDeleted",
					table: "Messages",
					type: "INTEGER",
					nullable: false,
					defaultValue: false);

			migrationBuilder.AddColumn<bool>(
					name: "SenderDeleted",
					table: "Messages",
					type: "INTEGER",
					nullable: false,
					defaultValue: false);
		}
	}
}
