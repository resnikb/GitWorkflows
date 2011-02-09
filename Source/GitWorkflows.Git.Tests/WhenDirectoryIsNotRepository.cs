using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests
{
    [TestFixture]
    public abstract class WhenDirectoryIsNotRepository
    {
        protected DirectoryInfo Root;

        [SetUp]
        public void CreateDirectory()
        {
            Root = AssemblyFixture.CreateTempDirectory();
        }

        protected void PopulateDirectory(string root, Dictionary<string, object> data)
        {
            foreach (var pair in data)
            {
                var path = Path.Combine(root, pair.Key);

                if (pair.Value == null || pair.Value is string)
                {
                    using (var f = new StreamWriter(path))
                    {
                        if (pair.Value != null)
                            f.Write((string)pair.Value);
                    }
                }
                else if (pair.Value is Dictionary<string, object>)
                {
                    Directory.CreateDirectory(path);
                    PopulateDirectory(path, (Dictionary<string, object>)pair.Value);
                }
            }
        }
    }
}
