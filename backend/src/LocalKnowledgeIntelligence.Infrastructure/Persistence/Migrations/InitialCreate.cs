using Microsoft.EntityFrameworkCore.Migrations;

namespace LocalKnowledgeIntelligence.Infrastructure;

[Microsoft.EntityFrameworkCore.Infrastructure.DbContextAttribute(typeof(AppDbContext))]
[Migration("202606190001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                username = table.Column<string>(maxLength: 80, nullable: false),
                password_hash = table.Column<string>(nullable: false),
                role = table.Column<string>(maxLength: 30, nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false),
                updated_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_users", x => x.id));

        migrationBuilder.CreateTable(
            name: "documents",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                title = table.Column<string>(maxLength: 200, nullable: false),
                source_type = table.Column<string>(maxLength: 50, nullable: false),
                content = table.Column<string>(nullable: false),
                summary = table.Column<string>(nullable: true),
                tags = table.Column<string>(type: "jsonb", nullable: false),
                metadata = table.Column<string>(type: "jsonb", nullable: false),
                status = table.Column<string>(maxLength: 30, nullable: false),
                chunking_status = table.Column<string>(maxLength: 30, nullable: false),
                embedding_status = table.Column<string>(maxLength: 30, nullable: false),
                created_by_user_id = table.Column<Guid>(nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false),
                updated_at = table.Column<DateTimeOffset>(nullable: false),
                deleted_at = table.Column<DateTimeOffset>(nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_documents", x => x.id));

        migrationBuilder.CreateTable(
            name: "document_chunks",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                document_id = table.Column<Guid>(nullable: false),
                chunk_index = table.Column<int>(nullable: false),
                content = table.Column<string>(nullable: false),
                token_estimate = table.Column<int>(nullable: false),
                metadata = table.Column<string>(type: "jsonb", nullable: false),
                embedding_model = table.Column<string>(nullable: true),
                embedding_dimensions = table.Column<int>(nullable: true),
                embedding_generated_at = table.Column<DateTimeOffset>(nullable: true),
                created_at = table.Column<DateTimeOffset>(nullable: false),
                updated_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_document_chunks", x => x.id);
                table.ForeignKey("fk_document_chunks_documents_document_id", x => x.document_id, "documents", "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("ALTER TABLE document_chunks ADD COLUMN embedding vector(1536) NULL;");

        migrationBuilder.CreateTable(
            name: "chat_sessions",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                title = table.Column<string>(maxLength: 200, nullable: false),
                user_id = table.Column<Guid>(nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false),
                updated_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_chat_sessions", x => x.id));

        migrationBuilder.CreateTable(
            name: "chat_messages",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                session_id = table.Column<Guid>(nullable: false),
                role = table.Column<string>(maxLength: 30, nullable: false),
                content = table.Column<string>(nullable: false),
                citations = table.Column<string>(type: "jsonb", nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_chat_messages", x => x.id));

        migrationBuilder.CreateTable(
            name: "agent_runs",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                session_id = table.Column<Guid>(nullable: false),
                user_message_id = table.Column<Guid>(nullable: false),
                status = table.Column<string>(maxLength: 30, nullable: false),
                model = table.Column<string>(maxLength: 100, nullable: false),
                search_mode = table.Column<string>(maxLength: 30, nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false),
                completed_at = table.Column<DateTimeOffset>(nullable: true)
            },
            constraints: table => table.PrimaryKey("pk_agent_runs", x => x.id));

        migrationBuilder.CreateTable(
            name: "agent_steps",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                agent_run_id = table.Column<Guid>(nullable: false),
                step_index = table.Column<int>(nullable: false),
                tool_type = table.Column<string>(maxLength: 50, nullable: false),
                input = table.Column<string>(type: "jsonb", nullable: false),
                output = table.Column<string>(type: "jsonb", nullable: false),
                duration_ms = table.Column<int>(nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_agent_steps", x => x.id);
                table.ForeignKey("fk_agent_steps_agent_runs_agent_run_id", x => x.agent_run_id, "agent_runs", "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "evaluation_questions",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                question = table.Column<string>(nullable: false),
                expected_answer_keywords = table.Column<string>(type: "jsonb", nullable: false),
                expected_document_titles = table.Column<string>(type: "jsonb", nullable: false),
                difficulty = table.Column<string>(maxLength: 30, nullable: false),
                created_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_evaluation_questions", x => x.id));

        migrationBuilder.CreateTable(
            name: "evaluation_runs",
            columns: table => new
            {
                id = table.Column<Guid>(nullable: false),
                search_mode = table.Column<string>(maxLength: 30, nullable: false),
                question_count = table.Column<int>(nullable: false),
                score = table.Column<double>(nullable: false),
                results = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                created_at = table.Column<DateTimeOffset>(nullable: false),
                completed_at = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("pk_evaluation_runs", x => x.id));

        migrationBuilder.CreateIndex("idx_users_username", "users", "username", unique: true);
        migrationBuilder.CreateIndex("idx_documents_status", "documents", "status");
        migrationBuilder.CreateIndex("idx_documents_deleted_at", "documents", "deleted_at");
        migrationBuilder.CreateIndex("idx_documents_created_at", "documents", "created_at");
        migrationBuilder.CreateIndex("idx_documents_tags", "documents", "tags").Annotation("Npgsql:IndexMethod", "gin");
        migrationBuilder.CreateIndex("idx_documents_metadata", "documents", "metadata").Annotation("Npgsql:IndexMethod", "gin");
        migrationBuilder.CreateIndex("idx_document_chunks_document_id", "document_chunks", "document_id");
        migrationBuilder.CreateIndex("idx_document_chunks_document_id_chunk_index", "document_chunks", new[] { "document_id", "chunk_index" }, unique: true);
        migrationBuilder.Sql("CREATE INDEX idx_document_chunks_embedding_hnsw ON document_chunks USING hnsw (embedding vector_cosine_ops);");
        migrationBuilder.Sql("CREATE INDEX idx_document_chunks_content_trgm ON document_chunks USING gin (content gin_trgm_ops);");
        migrationBuilder.CreateIndex("idx_evaluation_questions_question", "evaluation_questions", "question", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("agent_steps");
        migrationBuilder.DropTable("chat_messages");
        migrationBuilder.DropTable("chat_sessions");
        migrationBuilder.DropTable("document_chunks");
        migrationBuilder.DropTable("evaluation_questions");
        migrationBuilder.DropTable("evaluation_runs");
        migrationBuilder.DropTable("agent_runs");
        migrationBuilder.DropTable("documents");
        migrationBuilder.DropTable("users");
    }
}
