using NUnit.Framework;

namespace GitWorkflows.Common.Tests
{
    [TestFixture]
    public class PathTests
    {
        [Test]
        [TestCase(@"c:\",          @"C:\")]
        [TestCase(@"c:\something", @"C:\SOMETHING")]
        [TestCase(@"c:\a\b\c",     @"c:\A\b\C")]
        [TestCase(@"relativePath", @"Relativepath")]
        [TestCase(@".\..\a\..\b",  @".\..\A\..\B")]
        [TestCase(@"c:\",          @"c:/")]
        [TestCase(@"c:\something", @"c:/something")]
        [TestCase(@"c:\a\b\c",     @"c:\A\b/c")]
        [TestCase(@"relativePath", @"Relativepath")]
        [TestCase(@".\..\a\..\b",  @".\..\A/../B")]
        [TestCase(@"c:\",          @"c:\.")]
        [TestCase(@"c:\",          @"c:\.\")]
        [TestCase(@"c:\something", @"c:\something\.\.\..\something")]
        [TestCase(@"c:\a\b\c",     @"c:\A\b\..\b\.\..\..\a\b\c")]
        [TestCase(@"relativePath", @"./Relativepath")]
        [TestCase(@".\..\a\..\b",  @"../b")]
        public void Equals_WhenPathsAreEquivalent_ReturnsTrue(string s1, string s2)
        {
            var path1 = new Path(s1);
            var path2 = new Path(s2);

            Assert.That(path1.Equals(path2));
            Assert.That(path2.Equals(path1));
            Assert.That(path1.Equals(s2));
            Assert.That(path2.Equals(s1));

            Assert.That(path1 == path2);
            Assert.That(path1 == s1);
            Assert.That(path1 == s2);
            Assert.That(s1 == path1);
            Assert.That(s2 == path1);
            Assert.That(path2 == path1);
            Assert.That(path2 == s1);
            Assert.That(path2 == s2);
            Assert.That(s1 == path2);
            Assert.That(s2 == path2);
        }

        [Test]
        [TestCase(@".\..\a\../B",     @"..\B")]
        [TestCase(@"relativePath",    @"relativePath")]
        [TestCase(@"./relativePath",  @"relativePath")]
        [TestCase(@"../x/../../Y",    @"..\..\Y")]
        [TestCase(@"x\..\Y",          @"Y")]
        [TestCase(@"C:\X\y\Z",        @"C:\X\y\Z")]
        [TestCase(@"C:\X\y\../Z",     @"C:\X\Z")]
        [TestCase(@"C:\",             @"C:\")]
        [TestCase(@"C:\..",           @"C:\")]
        public void ActualPath_IsNormalized(string input, string expected)
        {
            var path = new Path(input);

            Assert.That(path.ActualPath, Is.EqualTo(expected).IgnoreCase);
        }

        [Test]
        [TestCase(@"a\b\c",      @"a",          @"b\c")]
        [TestCase(@"C:\a\b\c",   @"C:\a\x\y",   @"..\..\b\c")]
        [TestCase(@"C:\a\b\c\d", @"C:\a\b\c",   @"d")]
        [TestCase(@"C:\a\b\c",   @"C:\a\b\c\d", @"..")]
        [TestCase(@"C:\a\b\c",   @"C:\a\b\c",   @".")]
        [TestCase(@"C:\..",      @"C:\",        @".")]
        public void GetRelativeTo_Tests(string p, string basePath, string expected)
        {
            var path = new Path(p);
            var relative = path.GetRelativeTo(basePath);

            Assert.That(relative.ActualPath, Is.EqualTo(expected));
        }

        [Test]
        [TestCase(@"a\b\c",      @"a\b",        @"a\b")]
        [TestCase(@"C:\a\b\c",   @"C:\a\x\y",   @"C:\a")]
        [TestCase(@"C:\a\b\c\d", @"C:\a\b\c",   @"C:\a\b\c")]
        [TestCase(@"C:\a\b\c\d", @"C:\x\y\z",   @"C:\")]
        [TestCase(@"C:\a\b\c\d", @"C:\x\y\z",   @"C:\")]
        [TestCase(@"C:\a\b\c\d", @"D:\x\y\z",   null)]
        public void GetCommonPrefix_Tests(string p, string basePath, string expected)
        {
            var path = new Path(p);
            var relative = path.GetCommonPrefix(basePath);

            if (expected == null)
                Assert.That(relative, Is.Null);
            else
                Assert.That(relative.ActualPath, Is.EqualTo(expected).IgnoreCase);
        }
    }
}
