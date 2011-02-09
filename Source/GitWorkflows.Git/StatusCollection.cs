using System.Collections.Generic;
using System.Diagnostics;
using GitWorkflows.Common;

namespace GitWorkflows.Git
{
    public class StatusCollection
    {
        private readonly Dictionary<Path, Status> _paths;

        public IEnumerable<Status> Statuses
        {
            get { return _paths.Values; }
        }

        public StatusCollection(IEnumerable<Status> statuses)
        {
            _paths = new Dictionary<Path, Status>();
            foreach (var status in statuses)
            {
                Status existingStatus;
                if (!_paths.TryGetValue(status.FilePath, out existingStatus))
                    _paths.Add(status.FilePath, status);
                else
                { 
                    existingStatus.FileStatus |= status.FileStatus;
                    if (status.RelatedStatus != null)
                    {
                        Debug.Assert(existingStatus.RelatedStatus == null, "Relating the same file with multiple different objects");
                        existingStatus.RelatedStatus = status.RelatedStatus;
                    }
                }
            }
        }

        public Status GetStatusOf(Path path)
        {
            Status status;
            if (_paths.TryGetValue(path, out status))
                return status;

            return null;
        }
    }
}