using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Git.Commands;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests
{
    [TestFixture]
    class WhenRepositoryIsEmpty : WhenDirectoryIsNotRepository
    {
        protected GitApplication Git;

        [SetUp]
        public void CreateRepository()
        {
            Git = new GitApplication(Root.FullName);
            Git.Execute(new Init());
        }

        [Test]
        public void Status_ReturnsEmptyCollection()
        {
            var result = Git.Execute(new Commands.Status());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void Status_WhenThereIsNewFile_ReturnsUntracked()
        {
            var contents = new Dictionary<string, object> {{"x", null}};
            PopulateDirectory(Git.WorkingDirectory, contents);

            var result = Git.Execute(new Commands.Status()).ToArray();
            var expected = new[]
            {
                new KeyValuePair<FileStatus, string>(FileStatus.Untracked, "x"),                    
            };

            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
