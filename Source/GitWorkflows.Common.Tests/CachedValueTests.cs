using System;
using System.Threading;
using NUnit.Framework;

namespace GitWorkflows.Common.Tests
{
    [TestFixture]
    public class CachedValueTests
    {
        [Test]
        public void Constructor_WhenHydrateFunctionIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new CachedValue<string>(null), Throws.InstanceOf<ArgumentNullException>());
        }
        
        [Test]
        public void Value_WhenFirstAccessed_IsHydrated()
        {
            const string value = "a value";
            var cache = new CachedValue<string>(() => value);

            Assert.That(cache.Value, Is.SameAs(value));
        }
        
        [Test]
        public void Value_WhenAccessedMultipleTimesWithoutInvalidating_HydratesOnlyOnce()
        {
            var numberOfHydrations = 0;
            var cache = new CachedValue<int>(() => ++numberOfHydrations);

            Assert.That(cache.Value, Is.EqualTo(1));
            Assert.That(cache.Value, Is.EqualTo(1));
            Assert.That(cache.Value, Is.EqualTo(1));

            Assert.That(numberOfHydrations, Is.EqualTo(1));
        }
        
        [Test]
        public void Value_WhenInvalidatedBeforeHydrating_HydratesOnce()
        {
            var numberOfHydrations = 0;
            var cache = new CachedValue<int>(() => ++numberOfHydrations);

            cache.Invalidate();
            Assert.That(cache.Value, Is.EqualTo(1));
            Assert.That(numberOfHydrations, Is.EqualTo(1));
        }        
        
        [Test]
        public void Value_WhenInvalidatedAfterHydrating_HydratesAgain()
        {
            var numberOfHydrations = 0;
            var cache = new CachedValue<int>(() => ++numberOfHydrations);

            Assert.That(cache.Value, Is.EqualTo(1));

            cache.Invalidate();

            Assert.That(cache.Value, Is.EqualTo(2));
            Assert.That(numberOfHydrations, Is.EqualTo(2));
        }
        
        [Test]
        public void HydratingFromMultipleThreads_IsSafe()
        {
            var numberOfHydrations = 0;
            var cache = new CachedValue<int>(() => ++numberOfHydrations);
            
            Assert.That(cache.Value, Is.EqualTo(1));

            using (var countdownEvent = new CountdownEvent(2))
            {
                Action threadAction = () =>
                {
                    cache.Invalidate();
                    countdownEvent.Signal();
                    countdownEvent.Wait();
                
                    Assert.That(cache.Value, Is.EqualTo(2));
                };

                var t1 = threadAction.BeginInvoke(threadAction.EndInvoke, null);
                var t2 = threadAction.BeginInvoke(threadAction.EndInvoke, null);
                WaitHandle.WaitAll(new[]{t1.AsyncWaitHandle, t2.AsyncWaitHandle});
            }

            Assert.That(numberOfHydrations, Is.EqualTo(2));
        }        
    }
}