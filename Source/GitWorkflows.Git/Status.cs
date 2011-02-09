using System;
using GitWorkflows.Common;

namespace GitWorkflows.Git
{
    [Flags]
    public enum FileStatus
    {
        Untracked         = 0x0001,
        Ignored           = 0x0002,
        NotModified       = 0x0004,
        Added             = 0x0008,
        Removed           = 0x0010,
        Modified          = 0x0020,
        CopySource        = 0x0040,
        CopyDestination   = 0x0080,
        RenameSource      = 0x0100,
        RenameDestination = 0x0200,
        Conflicted        = 0x0400,
    }

    public sealed class Status : IEquatable<Status>
    {
        public Path FilePath 
        { get; private set; }
        
        public FileStatus FileStatus
        { get; internal set; }

        public Status RelatedStatus
        { get; internal set; }

        public Status(Path filePath, FileStatus fileStatus, Status relatedStatus = null)
        {
            Arguments.EnsureNotNull(new{ filePath });

            FilePath = filePath;
            FileStatus = fileStatus;
            RelatedStatus = relatedStatus;
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