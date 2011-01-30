using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Package.Git;
using GitWorkflows.Package.Git.Commands;
using NUnit.Framework;
using Status = GitWorkflows.Package.Git.Commands.Status;

namespace GitWorkflows.Package.Tests.GitTests
{
    [TestFixture]
    public class WhenRepositoryIsEmpty : WhenDirectoryIsNotRepository
    {
        protected GitApplication Git;
        protected WorkingTree WorkingTree;

        [SetUp]
        public void CreateRepository()
        {
            Git = new GitApplication(Root.FullName);
            Git.Execute(new Init());

            WorkingTree = new WorkingTree(Root.FullName);
        }

        [Test]
        public void Status_ReturnsEmptyCollection()
        {
            var result = Git.Execute(new Status());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Status_WhenThereIsNewFile_ReturnsUntracked()
        {
            var contents = new Dictionary<string, object> {{"x", null}};
            PopulateDirectory(Git.WorkingDirectory, contents);

            var result = Git.Execute(new Status()).ToArray();
            var expected = new[]
            {
                new KeyValuePair<FileStatus, string>(FileStatus.Untracked, "x"),                    
            };

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}