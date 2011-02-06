using System;
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

    public sealed class Status : IEquatable<Status>
    {
        public Path FilePath 
        { get; private set; }
        
        public FileStatus FileStatus
        { get; private set; }

        public Status RelatedStatus
        { get; private set; }

        public Status(Path filePath, FileStatus fileStatus, Status relatedStatus = null)
        {
            Arguments.EnsureNotNull(new{ filePath });

            FilePath = filePath;
            FileStatus = fileStatus;
            RelatedStatus = relatedStatus;

            if (RelatedStatus != null)
                RelatedStatus.RelatedStatus = this;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter;
        /// otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Status other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return FileStatus == other.FileStatus && FilePath == other.FilePath;
        }

        public override bool Equals(object obj)
        { return Equals(obj as Status); }

        public override int GetHashCode()
        { return FileStatus.GetHashCode() ^ FilePath.GetHashCode(); }

        public override string ToString()
        { return string.Format("[{0}] {1}", FileStatus, FilePath); }
    }
}