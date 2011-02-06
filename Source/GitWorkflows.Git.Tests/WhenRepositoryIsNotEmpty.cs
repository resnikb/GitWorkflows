using NUnit.Framework;

namespace GitWorkflows.Git.Tests
{
    [TestFixture]
    class WhenRepositoryIsNotEmpty : WhenRepositoryIsEmpty
    {
//        [SetUp]
//        public void PopulateRepository()
//        {
//            var contents = new Dictionary<string, object>
//            {
//                {"a.txt", null},
//                {"b.txt", null},
//                {"dir1", new Dictionary<string, object> {
//                             {"dir1_a.txt", null}
//                }},
//            };
//
//            PopulateDirectory(Git.WorkingDirectory, contents);
//            Git.Execute(new Add {Target = Add.AddTarget.AllChanges});
//            Git.Execute(new Commit { Message = "test" });
//
//            RaiseSolutionOpening();
//        }
//
//        [Test]
//        public void WorkingTreeGetStatusOf_WhenFileIsTrackedAndNotModified_ReturnsNotModified()
//        {
//            var status = GitService.GetStatusOf("a.txt");
//
//            Assert.That(status, Is.EqualTo(FileStatus.NotModified));
//        }
//
//        [Test]
//        public void WorkingTreeGetStatusOf_WhenFileIsTrackedAndModified_ReturnsModified()
//        {
//            var contents = new Dictionary<string, object> {{"a.txt", "x"}};
//            PopulateDirectory(Git.WorkingDirectory, contents);
//
//            var status = GitService.GetStatusOf("a.txt");
//
//            Assert.That(status, Is.EqualTo(FileStatus.Modified));
//        }
//
//        [Test]
//        public void WorkingTreeGetStatusOf_WhenFileIsTrackedAndModifiedAndAccessedByFullPath_ReturnsModified()
//        {
//            var contents = new Dictionary<string, object> {{"a.txt", "x"}};
//            PopulateDirectory(Git.WorkingDirectory, contents);
//
//            var status = GitService.GetStatusOf(Path.Combine(Git.WorkingDirectory, "a.txt"));
//
//            Assert.That(status, Is.EqualTo(FileStatus.Modified));
//        }
//
//        [Test]
//        public void WorkingTreeGetStatusOf_WhenFileIsNotTracked_ReturnsUntracked()
//        {
//            var contents = new Dictionary<string, object> {{"untracked.txt", "x"}};
//            PopulateDirectory(Git.WorkingDirectory, contents);
//
//            var status = GitService.GetStatusOf("untracked.txt");
//
//            Assert.That(status, Is.EqualTo(FileStatus.Untracked));
//        }
//
//        [Test]
//        public void WorkingTreeGetStatusOf_WhenFileIsIgnored_ReturnsIgnored()
//        {
//            var contents = new Dictionary<string, object>
//                           {
//                               {"ignored.txt", "x"},
//                               {".gitignore", "ignored.txt"}
//                           };
//            PopulateDirectory(Git.WorkingDirectory, contents);
//
//            var status = GitService.GetStatusOf("ignored.txt");
//
//            Assert.That(status, Is.EqualTo(FileStatus.Ignored));
//        }
//
//        [Test]
//        public void WorkingTreeGetStatusOf_WhenFileIsRenamed_ReturnsRenamed()
//        {
//            var sourceFileName = Path.Combine(Git.WorkingDirectory, "a.txt");
//            var destFileName = Path.Combine(Git.WorkingDirectory, "renamed.a");
//
//            File.Move(sourceFileName, destFileName);
//            Git.Execute(new Add {Target = Add.AddTarget.AllChanges});
//
//            Assert.That(GitService.GetStatusOf(sourceFileName), Is.EqualTo(FileStatus.RenameSource));
//            Assert.That(GitService.GetStatusOf(destFileName), Is.EqualTo(FileStatus.RenameDestination));
//        }
    }
}