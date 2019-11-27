﻿using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using MoreLinq;

using NUnit.Framework;

using SKBKontur.EDIFunctionalTests.SqlStorageCoreTests.TestEntities;

namespace SKBKontur.EDIFunctionalTests.SqlStorageCoreTests
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
            var update = Task.Run(() =>
                {
                    sqlStorage.Batch(storage =>
                        {
                            storage.CreateOrUpdate<TestBatchStorageElement, Guid>(expected);
                            updateWaitHandle.Set();
                            deleteWaitHandle.WaitOne();
                            var actual = storage.TryRead<TestBatchStorageElement, Guid>(expected.Id);
                            actual.Should().BeEquivalentTo(expected);
                        }, isolationLevel);
                });
            var delete = Task.Run(() =>
                {
                    updateWaitHandle.WaitOne();
                    sqlStorage.Delete(expected.Id);
                    deleteWaitHandle.Set();
                });
            Task.WaitAll(new[] {update, delete}, TimeSpan.FromSeconds(5));
        }

        [Test]
        public void TestCreateAndMultipleUpdate()
        {
            var createWaitHandle = new ManualResetEvent(false);
            var entity = GenerateObjects().First();
            var expected = GenerateObjects(20).ToArray();
            expected.ForEach(e => e.Id = entity.Id);
            var updateTasks = expected.Select(e => Task.Run(() =>
                {
                    createWaitHandle.WaitOne();
                    sqlStorage.CreateOrUpdate(e);
                }));
            var create = Task.Run(() =>
                {
                    sqlStorage.Batch(storage =>
                        {
                            storage.CreateOrUpdate<TestBatchStorageElement, Guid>(entity);
                            createWaitHandle.Set();
                            var actual = storage.TryRead<TestBatchStorageElement, Guid>(entity.Id);
                            actual.Should().BeEquivalentTo(entity);
                        }, isolationLevel);
                });

            var runningTasks = updateTasks.Concat(new[] {create}).ToArray();
            Task.WaitAll(runningTasks, TimeSpan.FromSeconds(60));
            runningTasks.All(t => t.IsCompleted).Should().BeTrue();

            var actualUpdated = sqlStorage.TryRead(entity.Id);
            actualUpdated?.Value.Should().NotBe(entity.Value);
            actualUpdated?.Value.Should().BeOneOf(expected.Select(e => e.Value));
        }

        private readonly IsolationLevel isolationLevel;
    }
}