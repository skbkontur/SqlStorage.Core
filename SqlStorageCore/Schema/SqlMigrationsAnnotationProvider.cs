using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.

namespace SkbKontur.SqlStorageCore.Schema
{
    public class SqlMigrationsAnnotationProvider : NpgsqlMigrationsAnnotationProvider
    {
        public SqlMigrationsAnnotationProvider(MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        public override IEnumerable<IAnnotation> For(IEntityType entityType)
        {
            foreach (var annotation in base.For(entityType))
                yield return annotation;

            if (entityType[SqlAnnotations.EventLogTrigger] is true)
                yield return new Annotation(SqlAnnotations.EventLogTrigger, true);
        }
    }
}