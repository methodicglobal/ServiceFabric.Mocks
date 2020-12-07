using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.ReliableCollections;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable PossibleInvalidOperationException

namespace ServiceFabric.Mocks.Tests.TransactionTests
{
    /// <summary>
    /// Test that 2 default locks can be granted at the same time.
    /// </summary>
    [TestClass]
    public class LockTests
    {
        [TestMethod]
        public async Task Lock_DefaultLock()
        {
            Lock<int> l = new Lock<int>();

            var result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);

            result = await l.Acquire(2, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        /// <summary>
        /// Test that Default look cannot be granted while an Update lock is held
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Lock_UpdateBlockDefaultLock()
        {
            Lock<int> l = new Lock<int>();

            await l.Acquire(1, LockMode.Update);

            await Assert.ThrowsExceptionAsync<TimeoutException>(
                async () =>
                {
                    await l.Acquire(2, LockMode.Default, timeout: TimeSpan.FromMilliseconds(10));
                }
            );
        }

        [TestMethod]
        public async Task Lock_UpdateGrantedAfterDefaultRelease()
        {
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);

            var task = l.Acquire(2, LockMode.Default);
            l.Release(1);
            result = await task;
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultGrantedAfterUpdateDowngrade()
        {
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Update);
            Assert.AreEqual(AcquireResult.Acquired, result);

            var task = l.Acquire(2, LockMode.Default);
            l.Downgrade(1);
            result = await task;
            Assert.AreEqual(AcquireResult.Acquired, result);
        }

        [TestMethod]
        public async Task Lock_DefaultUpgradeNotDowngraded()
        {
            Lock<int> l = new Lock<int>();
            AcquireResult result;

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Acquired, result);
            Assert.AreEqual(l.LockMode, LockMode.Default);

            result = await l.Acquire(1, LockMode.Update);
            Assert.AreEqual(AcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);

            result = await l.Acquire(1, LockMode.Default);
            Assert.AreEqual(AcquireResult.Owned, result);
            Assert.AreEqual(l.LockMode, LockMode.Update);
        }

        [TestMethod]
        public async Task Lock_UpdateBlockedAndCancelled()
        {
            Lock<int> l = new Lock<int>();

            await l.Acquire(1, LockMode.Update);

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(
                async () =>
                {
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    var task = l.Acquire(2, LockMode.Default, cancellationToken: tokenSource.Token);
                    tokenSource.Cancel();
                    await task;
                }
            );
        }

        [TestMethod]
        public async Task LockManager_BasicTest()
        {
            LockManager<int, int> lockManager = new LockManager<int, int>();
            await lockManager.AcquireLock(1, 1, LockMode.Update);
            await lockManager.AcquireLock(1, 2, LockMode.Update);
            await lockManager.AcquireLock(1, 3, LockMode.Update);

            Task[] tasks = new Task[]
                {
                    lockManager.AcquireLock(2, 1, LockMode.Update),
                    lockManager.AcquireLock(3, 2, LockMode.Update),
                    lockManager.AcquireLock(4, 3, LockMode.Update),
                };

            lockManager.ReleaseLocks(1);
            Task.WaitAll(tasks);

            lockManager.ReleaseLocks(2);
            lockManager.ReleaseLocks(3);

            await lockManager.AcquireLock(5, 1, LockMode.Default);
            await lockManager.AcquireLock(5, 2, LockMode.Default);
            await Assert.ThrowsExceptionAsync<TimeoutException>(
                async () =>
                {
                    await lockManager.AcquireLock(5, 3, LockMode.Default, timeout: TimeSpan.FromMilliseconds(10));
                }
            );
        }


        [TestMethod]
        public void Lock_RaceToAcquire_Success()
        {
            var waitToStart = new ManualResetEventSlim(false);
            var waitToEnd = new ManualResetEventSlim(false);
            AcquireResult? resultA = null;
            AcquireResult? resultB = null;
            Lock<int> l = new Lock<int>();

            Thread a = new Thread(state =>
            {
                l.Acquire(1, LockMode.Default, 100, new CancellationToken(false)).Wait();
                resultA = l.Acquire(1, LockMode.Default, 100, new CancellationToken(false)).Result;
                waitToStart.Set(); //run b

                waitToEnd.Wait(); //wait for b
                l.Release(1);
            });

            Thread b = new Thread(state =>
            {
                waitToStart.Wait();
                waitToEnd.Set(); //continue a
                resultB = l.Acquire(2, LockMode.Update, 100, new CancellationToken(false)).Result; //wait for the lock for 100ms
            });

            a.Start();
            b.Start();

            a.Join();
            b.Join();

            Assert.IsTrue(resultA.HasValue);
            Assert.IsTrue(resultB.HasValue);
            Assert.AreEqual(AcquireResult.Owned, resultA.Value);
            Assert.AreEqual(AcquireResult.Acquired, resultB.Value);
        }

        [TestMethod]
        public void Lock_RaceToAcquire_A_Delays_Success()
        {
            var waitToStart = new ManualResetEventSlim(false);
            var waitToEnd = new ManualResetEventSlim(false);
            AcquireResult? resultA = null;
            AcquireResult? resultB = null;
            Lock<int> l = new Lock<int>();

            Thread a = new Thread(state =>
            {
                l.Acquire(1, LockMode.Default, 100, new CancellationToken(false)).Wait();
                resultA = l.Acquire(1, LockMode.Default, 100, new CancellationToken(false)).Result;
                waitToStart.Set(); //run b

                waitToEnd.Wait(); //wait for b
                Thread.Sleep(90); //keep the lock for 90ms
                l.Release(1);
            });

            Thread b = new Thread(state =>
            {
                waitToStart.Wait();
                waitToEnd.Set(); //continue a
                resultB = l.Acquire(2, LockMode.Update, 200, new CancellationToken(false)).Result; //wait for the lock for 200ms
            });

            a.Start();
            b.Start();

            a.Join();
            b.Join();

            Assert.IsTrue(resultA.HasValue);
            Assert.IsTrue(resultB.HasValue);
            Assert.AreEqual(AcquireResult.Owned, resultA.Value);
            Assert.AreEqual(AcquireResult.Acquired, resultB.Value);
        }

        [TestMethod]
        public void Lock_RaceToAcquire_Fail()
        {
            var waitToStart = new ManualResetEventSlim(false);
            var waitToEnd = new ManualResetEventSlim(false);
            AcquireResult? resultA = null;
            AcquireResult? resultB = null;
            Lock<int> l = new Lock<int>();

            Thread a = new Thread(state =>
            {
                l.Acquire(1, LockMode.Default, 100, new CancellationToken(false)).Wait();
                resultA = l.Acquire(1, LockMode.Default, 100, new CancellationToken(false)).Result;
                waitToStart.Set(); //run b

                waitToEnd.Wait(); //wait for b
                Thread.Sleep(110); //keep the lock for 110ms
                l.Release(1);
            });

            Thread b = new Thread(state =>
            {
                waitToStart.Wait();
                waitToEnd.Set(); //continue a
                resultB = l.Acquire(2, LockMode.Update, 100, new CancellationToken(false)).Result; //wait for the lock for 100ms
            });

            a.Start();
            b.Start();

            a.Join();
            b.Join();

            Assert.IsTrue(resultA.HasValue);
            Assert.IsTrue(resultB.HasValue);
            Assert.AreEqual(AcquireResult.Owned, resultA.Value);
            Assert.AreEqual(AcquireResult.Denied, resultB.Value);
        }
    }

    [TestClass]
    public class TestDeadLock
    {
        public class Counter
        {
            private IReliableStateManager stateManager;
            private const string counterName = "MyCounter";
            public Counter(IReliableStateManager stateManager)
            {
                this.stateManager = stateManager;
            }

            public async Task<IDictionary<uint, uint>> GetAsync(ITransaction tx, string objectKey)
            {
                var graphCounter =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<string, ImmutableDictionary<uint, uint>>>(counterName);
                ConditionalValue<ImmutableDictionary<uint, uint>> counterContainer = await graphCounter.TryGetValueAsync(tx, objectKey);
                return counterContainer.HasValue ? counterContainer.Value : ImmutableDictionary<uint, uint>.Empty;
            }

            /// <inheritdoc cref="IGraphCounter.GetAsync(ITransaction, string, Counter)" />
            public async Task<uint> GetAsync(ITransaction tx, string objectKey, uint counter)
            {
                IDictionary<uint, uint> counterDictionary = await this.GetAsync(tx, objectKey);
                return counterDictionary.TryGetValue(counter, out uint counterValue) ? counterValue : 0;
            }

            public async Task IncrementAsync(ITransaction tx, string objectKey, uint counter, uint count = 1)
            {
                var graphCounter =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<string, ImmutableDictionary<uint, uint>>>(counterName);
                ConditionalValue<ImmutableDictionary<uint, uint>> counterDictionaryContainer = await graphCounter.TryGetValueAsync(tx, objectKey, LockMode.Update);
                ImmutableDictionary<uint, uint> counterDictionary;
                if (counterDictionaryContainer.HasValue)
                {
                    counterDictionary = counterDictionaryContainer.Value;
                    if (counterDictionary.TryGetValue(counter, out uint value))
                    {
                        counterDictionary = counterDictionary.SetItem(counter, value + count);
                    }
                    else
                    {
                        counterDictionary = counterDictionary.Add(counter, count);
                    }
                }
                else
                {
                    counterDictionary = ImmutableDictionary<uint, uint>.Empty.Add(counter, count);
                }

                await graphCounter.SetAsync(tx, objectKey, counterDictionary);
            }
        }

        /// <summary>
        /// StateManager mock.
        /// </summary>
        private MockReliableStateManager mockStateManager;

        /// <summary>
        /// The graph counters dictionary
        /// </summary>
        private System.Collections.Generic.Dictionary<uint, Counter> counters;

        /// <summary>
        /// The transaction
        /// </summary>
        private ITransaction _tx;

        /// <summary>
        /// The object key
        /// </summary>
        private string objectKey;

        [TestInitialize]
        public Task TestInitialize()
        {
            this.mockStateManager = new MockReliableStateManager();
            var usersGraphCounter = new Counter(this.mockStateManager);
            var groupsGraphCounter = new Counter(this.mockStateManager);
            this._tx = this.mockStateManager.CreateTransaction();
            this.objectKey = Guid.NewGuid().ToString();
            this.counters = new System.Collections.Generic.Dictionary<uint, Counter>()
            {
                { 1, usersGraphCounter },
                { 2, usersGraphCounter },
                { 3, usersGraphCounter },
                { 4, groupsGraphCounter }
            };
            return Task.FromResult(true);
        }

        [TestMethod]
        public async Task TestIncrement()
        {
            foreach (KeyValuePair<uint, Counter> kv in this.counters)
            {
                uint counter = kv.Key;
                Counter graphCounter = kv.Value;
                uint returnedValue = await graphCounter.GetAsync(this._tx, this.objectKey, counter);
                Assert.AreEqual(0u, returnedValue);
                await graphCounter.IncrementAsync(this._tx, this.objectKey, counter, 1);
                returnedValue = await graphCounter.GetAsync(this._tx, this.objectKey, counter);
                Assert.AreEqual(1u, returnedValue);
                await graphCounter.IncrementAsync(this._tx, this.objectKey, counter, 100);
                returnedValue = await graphCounter.GetAsync(this._tx, this.objectKey, counter);
                Assert.AreEqual(101u, returnedValue);

                await _tx.CommitAsync();

                // Now do the multi threading confirm it is safe
                const int range = 100;
                var tasks = new List<Task>();
                using (var ctk = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
                {
                    for (int i = 0; i < range; i++)
                    {
                        int i2 = i;
                        
                        Task t = Task.Run(
                            async () =>
                            {
                                try
                                {
                                    using (var tx = this.mockStateManager.CreateTransaction())
                                    {
                                        await graphCounter.IncrementAsync(tx, this.objectKey, counter, 1);
                                        await tx.CommitAsync();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"iteration {i2} - error{e.Message}");
                                    throw;
                                }
                            }, ctk.Token);
                        tasks.Add(t);
                    }

                    await Task.WhenAll(tasks.ToArray());
                }
                this._tx = this.mockStateManager.CreateTransaction();
                returnedValue = await graphCounter.GetAsync(this._tx, this.objectKey, counter);
                Assert.AreEqual(201u, returnedValue);
            }
        }
    }
}
