using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace GitWorkflows.Package.Tests.GitTests
{
    [TestFixture]
    public abstract class WhenDirectoryIsNotRepository
    {
        private readonly List<DirectoryInfo> _toDelete = new List<DirectoryInfo>();

        protected DirectoryInfo Root;

        [SetUp]
        public void CreateDirectory()
        {
            Root = CreateTempDirectory();
        }

        [TearDown]
        public void RemoveAllTempDirectories()
        {
            foreach (var info in _toDelete)
            {
                try
                {
                    info.Delete(true);
                }
                catch
                {
                    // Ignore
                }
            }
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

        protected DirectoryInfo CreateTempDirectory()
        {
            var tempPath = Path.GetTempPath();
            while (true)
            {
                try
                {
                    var directoryPath = Path.Combine(tempPath, Path.GetRandomFileName());
                    var result = Directory.CreateDirectory(directoryPath);
                    _toDelete.Add(result);
                    return result;
                }
                catch
                {
                    continue;
                }
            }
        }
    }
}