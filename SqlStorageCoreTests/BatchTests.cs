using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using MoreLinq;

using NUnit.Framework;

using SkbKontur.SqlStorageCore.Tests.TestEntities;

namespace SkbKontur.SqlStorageCore.Tests
{
    [TestFixture(IsolationLevel.ReadCommitted)]
    [TestFixture(IsolationLevel.Serializable)]
    public class BatchTests : SqlStorageTestBase<TestBatchStorageElement, Guid>
    {
        public BatchTests(IsolationLevel isolationLevel)
        {
            this.isolationLevel = isolationLevel;
        }

        [Test]
        public void TestDeleteWhileCreateAndRetrieve()
        {
            var updateWaitHandle = new AutoResetEvent(false);
            var deleteWaitHandle = new AutoResetEvent(false);
            var expected = GenerateObjects().First();
            var update = Task.Run(async () => await sqlStorage.BatchAsync(async storage =>
                {
                    await storage.CreateOrUpdateAsync<TestBatchStorageElement, Guid>(expected);
                    updateWaitHandle.Set();
                    deleteWaitHandle.WaitOne();
                    var actual = await storage.TryReadAsync<TestBatchStorageElement, Guid>(expected.Id);
                    actual.Should().BeEquivalentTo(expected);
                }, isolationLevel));
            var delete = Task.Run(async () =>
                {
                    updateWaitHandle.WaitOne();
                    await sqlStorage.DeleteAsync(expected.Id);
                    deleteWaitHandle.Set();
                });
            Task.WaitAll(new[] {update, delete}, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task TestCreateAndMultipleUpdate()
        {
            var createWaitHandle = new ManualResetEvent(false);
            var entity = GenerateObjects().First();
            var expected = GenerateObjects(20).ToArray();
            expected.ForEach(e => e.Id = entity.Id);
            var updateTasks = expected.Select(e => Task.Run(async () =>
                {
                    createWaitHandle.WaitOne();
                    await sqlStorage.CreateOrUpdateAsync(e);
                }));
            var create = Task.Run(() =>
                {
                    sqlStorage.BatchAsync(async storage =>
                        {
                            await storage.CreateOrUpdateAsync<TestBatchStorageElement, Guid>(entity);
                            createWaitHandle.Set();
                            var actual = await storage.TryReadAsync<TestBatchStorageElement, Guid>(entity.Id);
                            actual.Should().BeEquivalentTo(entity);
                        }, isolationLevel);
                });

            var runningTasks = updateTasks.Concat(new[] {create}).ToArray();
            Task.WaitAll(runningTasks, TimeSpan.FromSeconds(60));
            runningTasks.All(t => t.IsCompleted).Should().BeTrue();

            var actualUpdated = await sqlStorage.TryReadAsync(entity.Id);
            actualUpdated?.Value.Should().NotBe(entity.Value);
            actualUpdated?.Value.Should().BeOneOf(expected.Select(e => e.Value));
        }

        private readonly IsolationLevel isolationLevel;
    }
}