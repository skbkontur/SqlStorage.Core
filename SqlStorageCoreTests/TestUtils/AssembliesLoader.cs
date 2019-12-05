using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using JetBrains.Annotations;

namespace SkbKontur.SqlStorageCore.Tests.TestUtils
{
    public static class AssembliesLoader
    {
        public static IEnumerable<Assembly> Load()
        {
            return EnumerateFiles()
                   .Where(IsOurAssemblyFile)
                   .Select(Assembly.LoadFrom)
                   .ToArray();
        }

        public static bool IsOurAssemblyFile(string fullFileName)
        {
            var fileName = Path.GetFileName(fullFileName);
            if (string.IsNullOrEmpty(fileName))
                return false;
            var extension = Path.GetExtension(fileName);
            return (extension.Equals(".dll", StringComparison.InvariantCultureIgnoreCase) ||
                    extension.Equals(".exe", StringComparison.InvariantCultureIgnoreCase)) &&
                   Path.GetFileNameWithoutExtension(fileName).StartsWith("SkbKontur.SqlStorageCore");
        }

        private static IEnumerable<string> EnumerateFiles()
        {
            var relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var relativeSearchPathFiles = string.IsNullOrEmpty(relativeSearchPath) ? new string[0] : Directory.EnumerateFiles(relativeSearchPath, "*", SearchOption.TopDirectoryOnly);

            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var baseDirectoryFiles = string.IsNullOrEmpty(baseDirectory) ? new string[0] : Directory.EnumerateFiles(baseDirectory, "*", SearchOption.TopDirectoryOnly);

            return baseDirectoryFiles.Concat(relativeSearchPathFiles).Distinct();
        }
    }
}