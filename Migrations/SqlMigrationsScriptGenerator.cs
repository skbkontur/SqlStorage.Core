using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations
{
    public class SqlMigrationsScriptGenerator : NpgsqlMigrationsSqlGenerator
    {
        public SqlMigrationsScriptGenerator([NotNull] MigrationsSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override void Generate(
            [NotNull] CreateTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder,
            bool terminate)
        {
            var shouldCreateTrigger = operation[SqlAnnotationsNames.EventLogTrigger] is true;

            base.Generate(operation, model, builder, terminate : !shouldCreateTrigger);

            if (shouldCreateTrigger)
            {
                var eventLogEntity = model?.FindEntityType(typeof(EventLogStorageElement));
                if (eventLogEntity == null)
                    throw new InvalidProgramStateException($"{nameof(EventLogStorageElement)} not found in model.");
                var eventLogTableName = eventLogEntity.Relational().TableName;
                if (string.IsNullOrEmpty(eventLogTableName))
                    throw new InvalidProgramStateException($"{nameof(EventLogStorageElement)} table name not found. Event log model: {eventLogEntity.ToDebugString(singleLine : false)}");
                AppendTriggerCreation(operation, builder, eventLogTableName);
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        private void AppendTriggerCreation([NotNull] CreateTableOperation operation, [NotNull] MigrationCommandListBuilder builder, [NotNull] string eventLogTableName)
        {
            var statementTerminator = Dependencies.SqlGenerationHelper.StatementTerminator;
            builder.AppendLine(statementTerminator);
            const string functionName = "write_modification_to_event_log";
            builder.AppendLine($"CREATE OR REPLACE FUNCTION {functionName}()");
            builder.AppendLine($"RETURNS TRIGGER AS ${functionName}$");
            builder.Append("DECLARE data json").AppendLine(statementTerminator);
            builder.AppendLine("BEGIN");
            builder.AppendLine("IF (TG_OP = 'DELETE') THEN");
            builder.Append("data = row_to_json(OLD)").AppendLine(statementTerminator);
            builder.AppendLine("ELSE");
            builder.Append("data = row_to_json(NEW)").AppendLine(statementTerminator);
            builder.Append("END IF").AppendLine(statementTerminator);
            builder.AppendLine($"INSERT INTO \"{eventLogTableName}\" (\"EntityType\", \"EntityContent\", \"Type\", \"Timestamp\")");
            builder.Append("VALUES (TG_TABLE_NAME, data, TG_OP, current_timestamp at time zone 'UTC')").AppendLine(statementTerminator);
            builder.AppendLine("IF (TG_OP = 'DELETE') THEN");
            builder.Append("RETURN OLD").AppendLine(statementTerminator);
            builder.AppendLine("ELSE");
            builder.Append("RETURN NEW").AppendLine(statementTerminator);
            builder.Append("END IF").AppendLine(statementTerminator);
            builder.Append("END").AppendLine(statementTerminator);
            builder.Append($"${functionName}$ language 'plpgsql'").AppendLine(statementTerminator);

            var triggerName = $"tr_update_{operation.Name}";
            var tableName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema);

            builder.Append($"DROP TRIGGER IF EXISTS {triggerName} ON {tableName}").AppendLine(statementTerminator);
            builder.AppendLine($"CREATE TRIGGER {triggerName}");
            builder.AppendLine($"BEFORE INSERT OR UPDATE OR DELETE ON {tableName}");
            builder.AppendLine("FOR EACH ROW");
            builder.Append($"EXECUTE PROCEDURE {functionName}()").AppendLine(statementTerminator);
        }
    }
}