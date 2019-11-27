using System;
using System.Linq;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

using SkbKontur.SqlStorageCore.EventLog;

namespace SkbKontur.SqlStorageCore.Schema
{
    [UsedImplicitly]
    public class SqlMigrationsScriptGenerator : NpgsqlMigrationsSqlGenerator
    {
        public SqlMigrationsScriptGenerator(MigrationsSqlGeneratorDependencies dependencies)
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

            ReplaceWriteToEventLogFunctionIfEventLogTableTouched(model, builder, targetTableName : operation.NewName);
        }

        protected override void Generate(
            [NotNull] RenameColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            ReplaceWriteToEventLogFunctionIfEventLogTableTouched(model, builder, targetTableName : operation.Table);
        }

        protected override void Generate(
            [NotNull] AddColumnOperation operation,
            [CanBeNull] IModel model,
            [NotNull] MigrationCommandListBuilder builder)
        {
            base.Generate(operation, model, builder);

            ReplaceWriteToEventLogFunctionIfEventLogTableTouched(model, builder, operation.Table);
        }

        private void ReplaceWriteToEventLogFunctionIfEventLogTableTouched([CanBeNull] IModel model, [NotNull] MigrationCommandListBuilder builder, [CanBeNull] string targetTableName)
        {
            var eventLogEntity = FindEventLogEntity(model);
            if (targetTableName == eventLogEntity.Relational().TableName)
            {
                builder.Append(Dependencies.SqlGenerationHelper.StatementTerminator);
                AppendCreateOrReplaceWriteToEventLogFunction(builder, eventLogEntity);
                builder.EndCommand(suppressTransaction : false);
            }
        }

        [NotNull]
        private static IEntityType FindEventLogEntity(IModel model)
        {
            var eventLogEntity = model?.FindEntityType(typeof(SqlEventLogEntry));
            if (eventLogEntity == null)
                throw new InvalidOperationException($"{nameof(SqlEventLogEntry)} not found in model.");
            return eventLogEntity;
        }

        private void AppendTriggerCreation([NotNull] CreateTableOperation operation, [NotNull] MigrationCommandListBuilder builder, [NotNull] IEntityType eventLogEntity)
        {
            var statementTerminator = Dependencies.SqlGenerationHelper.StatementTerminator;

            builder.AppendLine(statementTerminator);
            AppendCreateOrReplaceWriteToEventLogFunction(builder, eventLogEntity);

            var triggerName = $"tr_update_{operation.Name}";
            var tableName = Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema);

            builder
                .Append($"DROP TRIGGER IF EXISTS {triggerName} ON {tableName}").AppendLine(statementTerminator)
                .AppendLine($"CREATE TRIGGER {triggerName}")
                .AppendLine($"AFTER INSERT OR UPDATE OR DELETE ON {tableName}")
                .AppendLine("FOR EACH ROW")
                .Append($"EXECUTE PROCEDURE {writeToEventLogFunctionName}()").AppendLine(statementTerminator);
        }

        private void AppendCreateOrReplaceWriteToEventLogFunction([NotNull] MigrationCommandListBuilder builder, [NotNull] IEntityType eventLogEntity)
        {
            var statementTerminator = Dependencies.SqlGenerationHelper.StatementTerminator;
            var eventLogTableName = eventLogEntity.Relational().TableName;

            if (string.IsNullOrEmpty(eventLogTableName))
                throw new InvalidOperationException($"{nameof(SqlEventLogEntry)} table name not found. Event log model: {eventLogEntity.ToDebugString(singleLine : false)}");
            const string rowDataVariableName = "data";

            var insertedColumnsMap = GetActiveColumnsMap(eventLogEntity, rowDataVariableName);

            builder
                .AppendLine($"CREATE OR REPLACE FUNCTION {writeToEventLogFunctionName}()")
                .AppendLine($"RETURNS TRIGGER AS ${writeToEventLogFunctionName}$")
                .Append($"DECLARE {rowDataVariableName} json")
                .AppendLine(statementTerminator)
                .AppendLine("BEGIN")
                .AppendLine("IF (TG_OP = 'DELETE') THEN")
                .Append($"{rowDataVariableName} = row_to_json(OLD)")
                .AppendLine(statementTerminator)
                .AppendLine("ELSE")
                .Append($"{rowDataVariableName} = row_to_json(NEW)")
                .AppendLine(statementTerminator)
                .Append("END IF")
                .AppendLine(statementTerminator)
                .AppendLine($"INSERT INTO \"{eventLogTableName}\" ({string.Join(", ", insertedColumnsMap.Select(i => $"\"{i.ColumnName}\""))})")
                .Append($"VALUES ({string.Join(", ", insertedColumnsMap.Select(i => i.ColumnValueExpression))})")
                .AppendLine(statementTerminator)
                .AppendLine("IF (TG_OP = 'DELETE') THEN")
                .Append("RETURN OLD")
                .AppendLine(statementTerminator)
                .AppendLine("ELSE")
                .Append("RETURN NEW")
                .AppendLine(statementTerminator)
                .Append("END IF")
                .AppendLine(statementTerminator)
                .Append("END")
                .AppendLine(statementTerminator)
                .Append($"${writeToEventLogFunctionName}$ language 'plpgsql'")
                .AppendLine(statementTerminator);
        }

        private (string ColumnName, string ColumnValueExpression)[] GetActiveColumnsMap([NotNull] IEntityType eventLogEntity, [NotNull] string rowDataVariableName)
        {
            var entityTypeColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.EntityType)).Relational().ColumnName;
            var entityContentColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.EntityContent)).Relational().ColumnName;
            var operationTypeColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.ModificationType)).Relational().ColumnName;
            var timestampColumnName = eventLogEntity.GetProperty(nameof(SqlEventLogEntry.Timestamp)).Relational().ColumnName;
            var transactionIdColumnName = eventLogEntity.FindProperty(nameof(SqlEventLogEntry.TransactionId))?.Relational().ColumnName;

            var currentTransactionTimestampTicksExpression = SqlCommonQueriesBuilder.TicksFromTimestamp(SqlCommonQueriesBuilder.CurrentTransactionTimestamp());
            var currentTransactionIdExpression = SqlCommonQueriesBuilder.CurrentTransactionId();

            return new[]
                       {
                           (entityTypeColumnName, "TG_TABLE_NAME"),
                           (entityContentColumnName, rowDataVariableName),
                           (operationTypeColumnName, "TG_OP"),
                           (timestampColumnName, currentTransactionTimestampTicksExpression),
                           (transactionIdColumnName, currentTransactionIdExpression),
                       }
                   .Where(t => !string.IsNullOrEmpty(t.Item1))
                   .ToArray();
        }

        private const string writeToEventLogFunctionName = "write_modification_to_event_log";
    }
}