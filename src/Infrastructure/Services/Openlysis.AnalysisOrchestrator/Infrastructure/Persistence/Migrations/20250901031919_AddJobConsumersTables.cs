using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Openlysis.AnalysisOrchestrator.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJobConsumersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_saga",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<int>(type: "integer", nullable: false),
                    submitted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    service_address = table.Column<string>(type: "text", nullable: true),
                    job_timeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    job = table.Column<string>(type: "text", nullable: true),
                    job_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_attempt = table.Column<int>(type: "integer", nullable: false),
                    started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    faulted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    job_slot_wait_token = table.Column<Guid>(type: "uuid", nullable: true),
                    job_retry_delay_token = table.Column<Guid>(type: "uuid", nullable: true),
                    incomplete_attempts = table.Column<string>(type: "text", nullable: true),
                    last_progress_value = table.Column<long>(type: "bigint", nullable: true),
                    last_progress_limit = table.Column<long>(type: "bigint", nullable: true),
                    last_progress_sequence_number = table.Column<long>(type: "bigint", nullable: true),
                    job_state = table.Column<string>(type: "text", nullable: true),
                    job_properties = table.Column<string>(type: "text", nullable: true),
                    cron_expression = table.Column<string>(type: "text", nullable: true),
                    time_zone_id = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    next_start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_saga", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "job_type_saga",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<int>(type: "integer", nullable: false),
                    active_job_count = table.Column<int>(type: "integer", nullable: false),
                    concurrent_job_limit = table.Column<int>(type: "integer", nullable: false),
                    override_job_limit = table.Column<int>(type: "integer", nullable: true),
                    override_limit_expiration = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    active_jobs = table.Column<string>(type: "text", nullable: true),
                    instances = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    global_concurrent_job_limit = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_type_saga", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "job_attempt_saga",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_attempt = table.Column<int>(type: "integer", nullable: false),
                    service_address = table.Column<string>(type: "text", nullable: true),
                    instance_address = table.Column<string>(type: "text", nullable: true),
                    started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    faulted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_check_token_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_attempt_saga", x => x.correlation_id);
                    table.ForeignKey(
                        name: "fk_job_attempt_saga_job_saga_job_id",
                        column: x => x.job_id,
                        principalTable: "job_saga",
                        principalColumn: "correlation_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_job_attempt_saga_job_id_retry_attempt",
                table: "job_attempt_saga",
                columns: new[] { "job_id", "retry_attempt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_attempt_saga");

            migrationBuilder.DropTable(
                name: "job_type_saga");

            migrationBuilder.DropTable(
                name: "job_saga");
        }
    }
}
