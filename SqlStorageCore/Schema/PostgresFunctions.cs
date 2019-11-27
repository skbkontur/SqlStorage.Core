using System;

using Microsoft.EntityFrameworkCore;

namespace SkbKontur.SqlStorageCore.Schema
{
    public static class PostgresFunctions
    {
        [DbFunction("txid_snapshot_xmin")]
        public static long SnapshotMinimalTransactionId(string transactionIdsSnapshot) => throw new NotImplementedException();

        [DbFunction("txid_current_snapshot")]
        // todo (iperevoschikov, 23.01.2019): actual result is txid_snapshot, didn't managed how to map it to custom CLR type
        public static string CurrentTransactionIdsSnapshot() => throw new NotImplementedException();
    }
}