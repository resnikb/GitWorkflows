using System.Collections.Generic;
using GitWorkflows.Common;
using GitWorkflows.Git;

namespace GitWorkflows.Services
{
    public interface IRepositoryService
    {
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