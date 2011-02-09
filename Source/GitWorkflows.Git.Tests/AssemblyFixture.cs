using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace GitWorkflows.Git.Tests
{
    [SetUpFixture]
    public class AssemblyFixture
    {
        private static readonly List<DirectoryInfo> _toDelete = new List<DirectoryInfo>();

        [TearDown]
        public static void RemoveAllTempDirectories()
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

        public static DirectoryInfo CreateTempDirectory()
        {
            var tempPath = Path.GetTempPath();
            while (true)
            {
                try
                {
                    var directoryPath = System.IO.Path.Combine(tempPath, System.IO.Path.GetRandomFileName());
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