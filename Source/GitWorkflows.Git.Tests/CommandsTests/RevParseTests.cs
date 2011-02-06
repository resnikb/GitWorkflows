using System;
using GitWorkflows.Common.Subprocess;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests.CommandsTests
{
    [TestFixture]
    public class RevParseTests
    {
        [Test]
        [TestCase("x")]
        [TestCase("x/y/z")]
        public void Parse_ReturnsOutput(string gitOutput)
        {
            var app = new ApplicationDefinition(null, string.Empty);
            var command = new Commands.RevParse();

            var result = command.ProcessResult(app, new Tuple<int, string>(0, gitOutput));
            Assert.That(result, Is.EqualTo(gitOutput));
        }
    }
}