using System;
using System.Linq;
using GitWorkflows.Common.Subprocess;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests.CommandsTests
{
    [TestFixture]
    public class StatusTests
    {
        [Test]
        [TestCase("A a.txt",          new[]{"a.txt"},          new[]{FileStatus.Added})]
        [TestCase("D a.txt",          new[]{"a.txt"},          new[]{FileStatus.Removed})]
        [TestCase("C a.txt -> b.txt", new[]{"a.txt", "b.txt"}, new[]{FileStatus.CopySource, FileStatus.CopyDestination})]
        [TestCase("R a.txt -> b.txt", new[]{"a.txt", "b.txt"}, new[]{FileStatus.RenameSource, FileStatus.RenameDestination})]
        [TestCase("M a.txt",          new[]{"a.txt"},          new[]{FileStatus.Modified})]
        [TestCase("U a.txt",          new[]{"a.txt"},          new[]{FileStatus.Conflicted})]
        [TestCase("= a.txt",          new[]{"a.txt"},          new[]{FileStatus.NotModified})]
        [TestCase("?? a.txt",         new[]{"a.txt"},          new[]{FileStatus.Untracked})]
        public void ParsingIndividualLines_WorksCorrectly(string statusLine, string[] parsedFiles, FileStatus[] parsedStatuses)
        {
            var app = new ApplicationDefinition(null, string.Empty);
            var command = new Commands.Status();

            var status = command.ProcessResult(app, new Tuple<int, string>(0, statusLine));

            var expected = parsedFiles.Zip(parsedStatuses, (name, fileStatus) => new Status(name, fileStatus)).ToArray();
            Assert.That(status.ToArray(), Is.EquivalentTo(expected));
        }

        [Test]
        public void Parsing_ProducesAbsolutePaths()
        {
            const string statusOutput = "A some/dir/inside/a.txt\n";

            var expected = new[]
            {
                new Status(@"C:\Project\some\dir\inside\a.txt", FileStatus.Added),                   
            };

            var app = new ApplicationDefinition(null, @"C:\Project");
            var command = new Commands.Status();

            var status = command.ProcessResult(app, new Tuple<int, string>(0, statusOutput));

            Assert.That(status.ToArray(), Is.EquivalentTo(expected));
        }

        [Test]
        public void Parsing_CompleteOutput_WorksCorrectly()
        {
            const string statusOutput =
                "A a.txt\n" +
                "R dir1/x -> dir2/y\n" +
                "?? readme\n";

            var expected = new[]
            {
                new Status(@"C:\Project\a.txt", FileStatus.Added),                   
                new Status(@"C:\Project\dir1\x", FileStatus.RenameSource),                   
                new Status(@"C:\Project\dir2\y", FileStatus.RenameDestination),                   
                new Status(@"C:\Project\readme", FileStatus.Untracked),                   
            };

            var app = new ApplicationDefinition(null, @"C:\Project");
            var command = new Commands.Status();

            var status = command.ProcessResult(app, new Tuple<int, string>(0, statusOutput));

            Assert.That(status.ToArray(), Is.EquivalentTo(expected));
        }
    }
}