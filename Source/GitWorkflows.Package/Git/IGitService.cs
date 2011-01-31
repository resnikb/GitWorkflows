namespace GitWorkflows.Package.Git
{
    interface IGitService : IBranchManager
    {
        bool IsRepositoryOpen
        { get; }

        FileStatus GetStatusOf(string path);
    }
}