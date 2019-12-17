using GroboContainer.Core;
using GroboContainer.Impl;

using SkbKontur.SqlStorageCore.Tests.TestUtils;

namespace SkbKontur.SqlStorageCore.Tests
{
    public static class GroboTestMachineryContainerConfigurator
    {
        public static ContainerConfiguration GetContainerConfiguration(string testSuiteName)
        {
            return new ContainerConfiguration(AssembliesLoader.Load(), testSuiteName, ContainerMode.UseShortLog);
        }
    }
}