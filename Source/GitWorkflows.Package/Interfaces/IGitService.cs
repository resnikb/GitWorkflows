using GitWorkflows.Package.FileSystem;
using GitWorkflows.Package.Git;

namespace GitWorkflows.Package.Interfaces
{
    interface IGitService
    {
        Path RepositoryRoot
        { get; }

        bool IsRepositoryOpen
        { get; }

        GitApplication Git
        { get; }

        FileStatus GetStatusOf(string path);
    }
}