using System.Collections.Generic;
using System.Linq;
using GitWorkflows.Common;

namespace GitWorkflows.Git
{
    public class StatusCollection
    {
        private readonly Dictionary<Path, Status> _paths;

        public StatusCollection(IEnumerable<Status> statuses)
        {
            _paths = statuses.ToDictionary(s => s.FilePath);
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