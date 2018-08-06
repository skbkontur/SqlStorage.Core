using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Migrations
{
    public class EntitiesMigrationsAnnotationProvider : NpgsqlMigrationsAnnotationProvider
    {
        public EntitiesMigrationsAnnotationProvider([NotNull] MigrationsAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        [NotNull]
        public override IEnumerable<IAnnotation> For([NotNull] IEntityType entityType)
        {
            foreach (var annotation in base.For(entityType))
            {
                yield return annotation;
            }

            if (entityType[EdiEntityAnnotationsNames.EventLogTrigger] is true)
                yield return new Annotation(EdiEntityAnnotationsNames.EventLogTrigger, true);
        }
    }
}