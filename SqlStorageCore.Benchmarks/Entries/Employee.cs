using System;

namespace SkbKontur.SqlStorageCore.Benchmarks.Entries
{
    public class Employee : SqlEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int PersonnelNumber { get; set; }
    }
}
