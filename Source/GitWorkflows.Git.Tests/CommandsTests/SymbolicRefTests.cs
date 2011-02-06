using System;
using GitWorkflows.Common.Subprocess;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests.CommandsTests
{
    [TestFixture]
    public class SymbolicRefTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        public void Setup_WhenNameIsInvalid_ThrowsInvalidOperationException(string name)
        {
            var app = new ApplicationDefinition(null, string.Empty);
            var command = new Commands.SymbolicRef();

            command.Name = name;
            
            Assert.That(() => command.Setup(app.CreateRunner()), Throws.InvalidOperationException);
        }

        [Test]
        [TestCase("abc", "abc")]
        [TestCase("refs/master", "master")]
        [TestCase("x/y/z/t/master", "master")]
        public void Parsing_ReturnsLastComponentOfOutput(string gitOutput, string expectedResult)
        {
            var app = new ApplicationDefinition(null, string.Empty);

            var command = new Commands.SymbolicRef();
            command.Name = "HEAD";
            
            var result = command.ProcessResult(app, new Tuple<int, string>(0, gitOutput));
            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}