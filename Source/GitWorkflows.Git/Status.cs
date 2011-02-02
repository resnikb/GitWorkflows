using GitWorkflows.Common;

namespace GitWorkflows.Git
{
    public enum FileStatus
    {
        Untracked,
        Ignored,
        NotModified,
        Added,
        Removed,
        Modified,
        CopySource,
        CopyDestination,
        RenameSource,
        RenameDestination,
        Conflicted,
    }

    public class Status
    {
        public Path FilePath 
        { get; private set; }
        
        public FileStatus FileStatus
        { get; private set; }

        public Status RelatedStatus
        { get; private set; }

        public Status(Path filePath, FileStatus fileStatus, Status relatedStatus = null)
        {
            FilePath = filePath;
            FileStatus = fileStatus;
            RelatedStatus = relatedStatus;

            if (RelatedStatus != null)
                RelatedStatus.RelatedStatus = this;
        }
    }
}