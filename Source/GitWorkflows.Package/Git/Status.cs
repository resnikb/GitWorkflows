using System;
using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Package.FileSystem;

namespace GitWorkflows.Package.Git
{
    public class Status
    {
        private readonly Lazy<Dictionary<FileStatus, Path[]>> _statuses;
        private readonly Lazy<Dictionary<Path, FileStatus>> _paths;

        public Status(IEnumerable<KeyValuePair<FileStatus, string>> statuses, string repositoryRoot)
        {
            _statuses = new Lazy<Dictionary<FileStatus, Path[]>>(
                () => statuses.GroupBy(p => p.Key)
                              .ToDictionary(g => g.Key, g => g.Select(p => new Path(p.Value)).ToArray())
            );

            _paths = new Lazy<Dictionary<Path, FileStatus>>(
                () => 
                {
                    var absolutePaths = statuses.Select(s => new KeyValuePair<FileStatus, string>(s.Key, System.IO.Path.Combine(repositoryRoot, s.Value)));

                    return statuses.Concat(absolutePaths).ToDictionary(p => new Path(p.Value), p => p.Key);
                }
            );
        }

        public IEnumerable<Path> GetPathsWith(FileStatus status)
        {
            Path[] paths;
            if (_statuses.Value.TryGetValue(status, out paths))
                return paths;

            return Enumerable.Empty<Path>();
        }

        public FileStatus GetStatusOf(Path path)
        {
            FileStatus status;
            if (_paths.Value.TryGetValue(path, out status))
                return status;

            return FileStatus.NotModified;
        }
    }
}