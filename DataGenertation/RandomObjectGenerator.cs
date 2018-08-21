using System;
using System.Collections.Generic;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.DataGenertation
{
    public class RandomObjectGenerator<T> where T : new()
    {
        public RandomObjectGenerator(Random random)
        {
            this.random = random;
        }

        private void UpdateObjectPropertiesWithRandomValues(T item)
        {
            Helper.FillWithRandomTrash(item, random, 100, 70, 10);
        }

        public IEnumerable<T> Generate()
        {
            while (true)
            {
                var result = new T();
                UpdateObjectPropertiesWithRandomValues(result);
                yield return result;
            }
        }

        private readonly Random random;
    }
}