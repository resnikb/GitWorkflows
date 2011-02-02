using System;
using GitWorkflows.Common;
using GitWorkflows.Git;

namespace GitWorkflows.Package.Interfaces
{
    interface IGitService
    {
        event EventHandler RepositoryChanged;
        event EventHandler WorkingTreeChanged;

        Path RepositoryRoot
        { get; }

        bool IsRepositoryOpen
        { get; }

        GitApplication Git
        { get; }

        Status GetStatusOf(string path);
    }
}