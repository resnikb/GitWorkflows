using System;
using System.Collections.Generic;
using System.ComponentModel;
using GitWorkflows.Common;
using GitWorkflows.Git;

namespace GitWorkflows.Services
{
    public interface IRepositoryService : INotifyPropertyChanged
    {
        event Action<HashSet<Path>> RepositoryChanged;

        GitApplication Git
        { get; }

        StatusCollection Status
        { get; }

        bool IsGitRepository
        { get; }

        Path BaseDirectory
        { get; }

        Path RepositoryDirectory
        { get; }

        void OpenRepositoryAt(Path path);
        void CloseRepository();
        void ResetChanges(IEnumerable<Path> files);
        void DisplayUnstagedChangesAsync(Path file);
    }
}