using System;
using System.Collections.Generic;
using System.Text;

namespace SkbKontur.SqlStorageCore.Linq
{
    internal static class BatchExtensions
    {
        public static List<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            var resultList = new List<IEnumerable<T>>();
            var currentBatch = new List<T>();
            foreach (var element in enumerable)
            {
                if (currentBatch.Count == batchSize)
                {
                    resultList.Add(currentBatch);
                    currentBatch = new List<T>();
                }
                else
                {
                    currentBatch.Add(element);
                }
            }
            if (currentBatch.Count > 0)
                resultList.Add(currentBatch);
            return resultList;
        }
    }
}
