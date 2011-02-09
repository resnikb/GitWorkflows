using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests
{
    [TestFixture]
    public class RepositoryMonitorTests
    {
        private const int EventTimeout = 3000;

        [Test]
        public void WhenFileIsCreated_RaisesWorkingTreeChangedEvent()
        {
            var dir = AssemblyFixture.CreateTempDirectory();

            var changedPaths = new HashSet<Common.Path>();

            using (var monitor = new RepositoryMonitor(dir.FullName))
            { 
                monitor.WorkingTreeChanged += changedPaths.UnionWith;

                using (var x = File.CreateText(Path.Combine(dir.FullName, "a")))
                    x.Write(Guid.NewGuid());

                SpinWait.SpinUntil(() => false, EventTimeout);

                var expected = new Common.Path[] {Path.Combine(dir.FullName, "a")};
                Assert.That(changedPaths.ToArray(), Is.EquivalentTo(expected));
            }
        }

        [Test]
        public void WhenFileIsCreatedInRepository_RaisesRepositoryChangedEvent()
        {
            var dir = AssemblyFixture.CreateTempDirectory();
            dir.CreateSubdirectory(".git");

            var changedPaths = new HashSet<Common.Path>();

            using (var monitor = new RepositoryMonitor(dir.FullName))
            { 
                monitor.RepositoryChanged += changedPaths.UnionWith;

                using (var x = File.CreateText(Path.Combine(dir.FullName, ".git", "a")))
                    x.Write(Guid.NewGuid());

                SpinWait.SpinUntil(() => false, EventTimeout);

                var expected = new Common.Path[] {Path.Combine(dir.FullName, ".git", "a")};
                Assert.That(changedPaths.ToArray(), Is.EquivalentTo(expected));
            }
        }

        [Test]
        public void WhenLockFileIsCreatedInRepository_DoesNotRaiseEvent()
        {
            var dir = AssemblyFixture.CreateTempDirectory();
            dir.CreateSubdirectory(".git");

            var changedPaths = new HashSet<Common.Path>();

            using (var monitor = new RepositoryMonitor(dir.FullName))
            { 
                monitor.RepositoryChanged += changedPaths.UnionWith;

                using (var x = File.CreateText(Path.Combine(dir.FullName, ".git", "a.lock")))
                    x.Write(Guid.NewGuid());

                SpinWait.SpinUntil(() => false, EventTimeout);

                Assert.That(changedPaths.ToArray(), Is.Empty);
            }
        }

        [Test]
        public void WhenLockFileIsCreatedInWorkingTree_RaisesWorkingTreeChangedEvent()
        {
            var dir = AssemblyFixture.CreateTempDirectory();

            var changedPaths = new HashSet<Common.Path>();

            using (var monitor = new RepositoryMonitor(dir.FullName))
            { 
                monitor.WorkingTreeChanged += changedPaths.UnionWith;

                using (var x = File.CreateText(Path.Combine(dir.FullName, "a.lock")))
                    x.Write(Guid.NewGuid());

                SpinWait.SpinUntil(() => false, EventTimeout);

                var expected = new Common.Path[] {Path.Combine(dir.FullName, "a.lock")};
                Assert.That(changedPaths.ToArray(), Is.EquivalentTo(expected));
            }
        }
    }
}