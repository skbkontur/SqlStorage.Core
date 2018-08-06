using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using JetBrains.Annotations;

using Microsoft.EntityFrameworkCore;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.Storage
{
    [UsedImplicitly]
    public class EntityStorage<T> : IEntityStorage<T>
        where T : class, IIdentifiableEntity
    {
        public EntityStorage(Func<EntitiesDatabaseContext> createDbContext)
        {
            this.createDbContext = createDbContext;
        }

        [NotNull, ItemNotNull]
        public T[] ReadAll()
        {
            using (var context = createDbContext())
            {
                return context.Set<T>().AsNoTracking().ToArray();
            }
        }

        [CanBeNull]
        public T TryRead(Guid id)
        {
            using (var context = createDbContext())
            {
                return context
                    .Set<T>()
                    .Find(id);
            }
        }

        [NotNull, ItemNotNull]
        public T[] TryRead([NotNull] Guid[] ids)
        {
            using (var context = createDbContext())
            {
                return context
                    .Set<T>()
                    .AsNoTracking()
                    .Where(e => ids.Contains(e.Id))
                    .ToArray();
            }
        }

        public void CreateOrUpdate([NotNull] T entity)
        {
            using (var context = createDbContext())
            {
                var existing = context.Set<T>().Find(entity.Id);
                if (existing == null)
                    context.Set<T>().Add(entity);
                else
                {
                    context.Entry(existing).State = EntityState.Detached;
                    context.Set<T>().Update(entity);
                }
                context.SaveChanges();
            }
        }

        public void CreateOrUpdate([NotNull, ItemNotNull] T[] entities)
        {
            using (var context = createDbContext())
            {
                var entitiesIds = entities.Select(e => e.Id).ToArray();
                var existing = context.Set<T>().AsNoTracking().Where(e => entitiesIds.Contains(e.Id)).Select(e => e.Id).ToArray();
                var entitiesToAdd = new List<T>();
                var entitiesToUpdate = new List<T>();
                foreach (var entity in entities)
                {
                    if (existing.Contains(entity.Id))
                        entitiesToUpdate.Add(entity);
                    else
                        entitiesToAdd.Add(entity);
                }
                context.Set<T>().AddRange(entitiesToAdd);
                context.Set<T>().UpdateRange(entitiesToUpdate);
                context.SaveChanges();
            }
        }

        public void Create([NotNull] T entity)
        {
            using (var context = createDbContext())
            {
                context.Set<T>().Add(entity);
                context.SaveChanges();
            }
        }

        public void Create([NotNull, ItemNotNull] T[] entities)
        {
            using (var context = createDbContext())
            {
                context.Set<T>().AddRange(entities);
                context.SaveChanges();
            }
        }

        public void Update([NotNull] T entity)
        {
            using (var context = createDbContext())
            {
                context.Set<T>().Update(entity);
                context.SaveChanges();
            }
        }

        public void Update([NotNull, ItemNotNull] T[] entities)
        {
            using (var context = createDbContext())
            {
                context.Set<T>().UpdateRange(entities);
                context.SaveChanges();
            }
        }

        public void Delete([NotNull] Guid[] ids)
        {
            using (var context = createDbContext())
            {
                var entities = context.Set<T>().AsNoTracking().Where(e => ids.Contains(e.Id));
                context.Set<T>().RemoveRange(entities);
                context.SaveChanges();
            }
        }

        public void Delete(Guid id)
        {
            using (var context = createDbContext())
            {
                var entity = context.Set<T>().Find(id);
                if (entity != null)
                {
                    context.Set<T>().Remove(entity);
                    context.SaveChanges();
                }
            }
        }

        public void Delete([NotNull] Expression<Func<T, bool>> criterion)
        {
            using (var context = createDbContext())
            {
                var entities = context.Set<T>().AsNoTracking().Where(criterion);
                context.Set<T>().RemoveRange(entities);
                context.SaveChanges();
            }
        }

        [NotNull, ItemNotNull]
        public T[] Find([NotNull] Expression<Func<T, bool>> criterion, int start, int limit)
        {
            using (var context = createDbContext())
            {
                return context
                    .Set<T>()
                    .AsNoTracking()
                    .Skip(start)
                    .Where(criterion)
                    .Take(limit)
                    .ToArray();
            }
        }

        public int GetCount([NotNull] Expression<Func<T, bool>> criterion)
        {
            using (var context = createDbContext())
            {
                return context.Set<T>().Count(criterion);
            }
        }

        [CanBeNull]
        public TValue GetMaxValue<TValue>([NotNull] Expression<Func<T, TValue>> propertySelector)
        {
            using (var context = createDbContext())
            {
                if (!context.Set<T>().Any())
                    return default(TValue);
                return context.Set<T>().Max(propertySelector);
            }
        }

        [CanBeNull]
        public TValue GetMaxValue<TValue>([NotNull] Expression<Func<T, TValue>> propertySelector, [NotNull] Expression<Func<T, bool>> filter)
        {
            using (var context = createDbContext())
            {
                if (!context.Set<T>().Any(filter))
                    return default(TValue);
                return context.Set<T>().Where(filter).Max(propertySelector);
            }
        }

        private readonly Func<EntitiesDatabaseContext> createDbContext;
    }
}