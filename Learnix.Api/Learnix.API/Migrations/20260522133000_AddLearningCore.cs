using System;
using Learnix.API.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Learnix.API.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260522133000_AddLearningCore")]
    public partial class AddLearningCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "best_streak_days",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "current_streak_days",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "daily_goal_minutes",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "grade",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_activity_date",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preparedness_level",
                table: "users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_xp",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    grades = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    icon_key = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    source_title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learning_levels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    grade = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(700)", maxLength: 700, nullable: true),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    source_title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learning_levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_learning_levels_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_subjects",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    selected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_subjects", x => new { x.user_id, x.subject_id });
                    table.ForeignKey(
                        name: "FK_user_subjects_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_subjects_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    learning_level_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    prompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    options_json = table.Column<string>(type: "jsonb", nullable: false),
                    correct_answer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    explanation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercises_learning_levels_learning_level_id",
                        column: x => x.learning_level_id,
                        principalTable: "learning_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_attempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    learning_level_id = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
                    correct_answers = table.Column<int>(type: "integer", nullable: false),
                    mistakes = table.Column<int>(type: "integer", nullable: false),
                    score_percent = table.Column<int>(type: "integer", nullable: false),
                    earned_xp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lesson_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lesson_attempts_learning_levels_learning_level_id",
                        column: x => x.learning_level_id,
                        principalTable: "learning_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lesson_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_level_progresses",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    learning_level_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    best_score_percent = table.Column<int>(type: "integer", nullable: false),
                    mistakes = table.Column<int>(type: "integer", nullable: false),
                    attempts_count = table.Column<int>(type: "integer", nullable: false),
                    earned_xp = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_level_progresses", x => new { x.user_id, x.learning_level_id });
                    table.ForeignKey(
                        name: "FK_user_level_progresses_learning_levels_learning_level_id",
                        column: x => x.learning_level_id,
                        principalTable: "learning_levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_level_progresses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_attempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    lesson_attempt_id = table.Column<int>(type: "integer", nullable: false),
                    exercise_id = table.Column<int>(type: "integer", nullable: false),
                    user_answer = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_exercise_attempts_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exercise_attempts_lesson_attempts_lesson_attempt_id",
                        column: x => x.lesson_attempt_id,
                        principalTable: "lesson_attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_exercise_attempts_lesson_exercise",
                table: "exercise_attempts",
                columns: new[] { "lesson_attempt_id", "exercise_id" });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_attempts_exercise_id",
                table: "exercise_attempts",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "ix_exercises_level_order",
                table: "exercises",
                columns: new[] { "learning_level_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_learning_levels_subject_grade_order",
                table: "learning_levels",
                columns: new[] { "subject_id", "grade", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lesson_attempts_user_level_completed",
                table: "lesson_attempts",
                columns: new[] { "user_id", "learning_level_id", "completed_at" });

            migrationBuilder.CreateIndex(
                name: "IX_lesson_attempts_learning_level_id",
                table: "lesson_attempts",
                column: "learning_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_code",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_level_progresses_learning_level_id",
                table: "user_level_progresses",
                column: "learning_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subjects_subject_id",
                table: "user_subjects",
                column: "subject_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "exercise_attempts");
            migrationBuilder.DropTable(name: "user_level_progresses");
            migrationBuilder.DropTable(name: "user_subjects");
            migrationBuilder.DropTable(name: "exercises");
            migrationBuilder.DropTable(name: "lesson_attempts");
            migrationBuilder.DropTable(name: "learning_levels");
            migrationBuilder.DropTable(name: "subjects");

            migrationBuilder.DropColumn(name: "best_streak_days", table: "users");
            migrationBuilder.DropColumn(name: "current_streak_days", table: "users");
            migrationBuilder.DropColumn(name: "daily_goal_minutes", table: "users");
            migrationBuilder.DropColumn(name: "grade", table: "users");
            migrationBuilder.DropColumn(name: "last_activity_date", table: "users");
            migrationBuilder.DropColumn(name: "preparedness_level", table: "users");
            migrationBuilder.DropColumn(name: "total_xp", table: "users");
        }
    }
}
