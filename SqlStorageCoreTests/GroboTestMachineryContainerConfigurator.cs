using GroboContainer.Core;
using GroboContainer.Impl;

using JetBrains.Annotations;

using SkbKontur.SqlStorageCore.Tests.TestUtils;

namespace SkbKontur.SqlStorageCore.Tests
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class GroboTestMachineryContainerConfigurator
    {
        [NotNull]
        public static ContainerConfiguration GetContainerConfiguration([NotNull] string testSuiteName)
        {
            return new ContainerConfiguration(AssembliesLoader.Load(), testSuiteName, ContainerMode.UseShortLog);
        }
    }
}