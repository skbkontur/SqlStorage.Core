using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

using SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog;
using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Schema
{
    [UsedImplicitly]
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
            var shouldCreateTrigger = operation[SqlAnnotations.EventLogTrigger] is true;

            base.Generate(operation, model, builder, terminate : !shouldCreateTrigger);

            if (shouldCreateTrigger)
            {
                var eventLogEntity = FindEventLogEntity(model);
                AppendTriggerCreation(operation, builder, eventLogEntity);
            }

            if (terminate)
            {
                builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                EndStatement(builder);
            }
        }

        protected override void Generate(
            [NotNull] RenameTableOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            var eventLogEntity = FindEventLogEntity(model);
            if (operation.NewName == eventLogEntity.Relational().TableName)
            {
                builder.Append(Dependencies.SqlGenerationHelper.StatementTerminator);
                AppendCreateOrReplaceWriteToEventLogFunction(builder, eventLogEntity);
                builder.EndCommand(suppressTransaction : false);
            }
        }

        protected override void Generate(
            [NotNull] RenameColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);
            var eventLogEntity = FindEventLogEntity(model);
            if (operation.Table == eventLogEntity.Relational().TableName)
            {
                builder.Append(Dependencies.SqlGenerationHelper.StatementTerminator);
                AppendCreateOrReplaceWriteToEventLogFunction(builder, eventLogEntity);
                builder.EndCommand(suppressTransaction : false);
            }
        }

        private static IEntityType FindEventLogEntity(IModel model)
        {
            var eventLogEntity = model?.FindEntityType(typeof(SqlEventLogEntry));
            if (eventLogEntity == null)
                throw new InvalidProgramStateException($"{nameof(SqlEventLogEntry)} not found in model.");
            return eventLogEntity;
        }

        private void AppendTriggerCreation([NotNull] CreateTableOperation operation, [NotNull] MigrationCommandListBuilder builder, [NotNull] IEntityType eventLogEntity)
        {
            var statementTerminator = Dependencies.SqlGenerationHelper.StatementTerminator;

            builder.AppendLine(statementTerminator);
            AppendCreateOrReplaceWriteToEventLogFunction(builder, eventLogEntity);

            var triggerName = $"tr_update_{operation.Name}";
            var tableName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema);

            builder.Append($"DROP TRIGGER IF EXISTS {triggerName} ON {tableName}").AppendLine(statementTerminator);
            builder.AppendLine($"CREATE TRIGGER {triggerName}");
            builder.AppendLine($"BEFORE INSERT OR UPDATE OR DELETE ON {tableName}");
            builder.AppendLine("FOR EACH ROW");
            builder.Append($"EXECUTE PROCEDURE {writeToEventLogFunctionName}()").AppendLine(statementTerminator);
        }

        private void AppendCreateOrReplaceWriteToEventLogFunction(MigrationCommandListBuilder builder, IEntityType eventLogEntity)
        {
            var statementTerminator = Dependencies.SqlGenerationHelper.StatementTerminator;
            var eventLogTableName = eventLogEntity.Relational().TableName;

            if (string.IsNullOrEmpty(eventLogTableName))
                throw new InvalidProgramStateException($"{nameof(SqlEventLogEntry)} table name not found. Event log model: {eventLogEntity.ToDebugString(singleLine : false)}");

            var entityTypeColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.EntityType)).Relational().ColumnName;
            var entityContentColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.EntityContent)).Relational().ColumnName;
            var operationTypeColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.ModificationType)).Relational().ColumnName;
            var timestampColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.Timestamp)).Relational().ColumnName;

            builder.AppendLine($"CREATE OR REPLACE FUNCTION {writeToEventLogFunctionName}()");
            builder.AppendLine($"RETURNS TRIGGER AS ${writeToEventLogFunctionName}$");
            builder.Append("DECLARE data json").AppendLine(statementTerminator);
            builder.AppendLine("BEGIN");
            builder.AppendLine("IF (TG_OP = 'DELETE') THEN");
            builder.Append("data = row_to_json(OLD)").AppendLine(statementTerminator);
            builder.AppendLine("ELSE");
            builder.Append("data = row_to_json(NEW)").AppendLine(statementTerminator);
            builder.Append("END IF").AppendLine(statementTerminator);
            builder.AppendLine($"INSERT INTO \"{eventLogTableName}\" (\"{entityTypeColumnName}\", \"{entityContentColumnName}\", \"{operationTypeColumnName}\", \"{timestampColumnName}\")");
            builder.Append($"VALUES (TG_TABLE_NAME, data, TG_OP, {SqlCommonQueriesBuilder.TicksFromTimestamp("transaction_timestamp()")})").AppendLine(statementTerminator);
            builder.AppendLine("IF (TG_OP = 'DELETE') THEN");
            builder.Append("RETURN OLD").AppendLine(statementTerminator);
            builder.AppendLine("ELSE");
            builder.Append("RETURN NEW").AppendLine(statementTerminator);
            builder.Append("END IF").AppendLine(statementTerminator);
            builder.Append("END").AppendLine(statementTerminator);
            builder.Append($"${writeToEventLogFunctionName}$ language 'plpgsql'").AppendLine(statementTerminator);
        }

        private const string writeToEventLogFunctionName = "write_modification_to_event_log";
    }
}