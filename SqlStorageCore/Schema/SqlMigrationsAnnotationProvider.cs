using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace SkbKontur.SqlStorageCore.Schema
{
    [UsedImplicitly]
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