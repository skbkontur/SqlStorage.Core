using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using SKBKontur.Catalogue.EDI.SqlStorageCore.DatabaseContext;
using SKBKontur.Catalogue.EDI.SqlStorageCore.Entities;

namespace SKBKontur.Catalogue.EDI.SqlStorageCore.EventLog
{
    public class EntitesStorageTimeRepository
    {
        public EntitesStorageTimeRepository(Func<EntitiesDatabaseContext> createContext)
        {
            this.createContext = createContext;
        }

        public DateTime GetCurrentStorageTime()
        {
            using (var context = createContext())
            {
                return DateTime.SpecifyKind(
                    context
                        .Query<DateTimeEntity>()
                        .FromSql("SELECT current_timestamp at time zone 'UTC' as \"Value\"")
                        .First()
                        .Value,
                    DateTimeKind.Utc);
            }
        }

        private readonly Func<EntitiesDatabaseContext> createContext;
    }
}