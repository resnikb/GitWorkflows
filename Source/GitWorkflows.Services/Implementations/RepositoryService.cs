using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Git;
using GitWorkflows.Git.Commands;
using GitWorkflows.Services.Events;
using NLog;
using Status = GitWorkflows.Git.Status;

namespace GitWorkflows.Services.Implementations
{
    [Export(typeof(IRepositoryService))]
    public class RepositoryService : IRepositoryService, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(RepositoryService).FullName);
        
        [Import]
        private GitRepositoryChangedEvent _repositoryChangedEvent;

        [Import]
        private GitWorkingTreeChangedEvent _workingTreeChangedEvent;

        private readonly CachedValue<StatusCollection> _status;
        private RepositoryMonitor _repositoryMonitor;
        private bool _disposed;

        public GitApplication Git
        { get; private set; }

        public StatusCollection Status
        {
            get { return _status.Value; }
        }

        public bool IsGitRepository
        { get; private set; }

        public Path BaseDirectory
        {
            get { return Git.WorkingDirectory; }
        }

        public Path RepositoryDirectory
        { get; private set; }

        public RepositoryService()
        {
            _status = new CachedValue<StatusCollection>(
                () =>
                {
                    if (!IsGitRepository)
                        return new StatusCollection(Enumerable.Empty<Status>());

                    var clean = new Clean
                    {
                        Target = Clean.CleanTarget.Ignored,
                        IncludeDirectories = false
                    };

                    var statusResult = Git.Execute(new Git.Commands.Status());
                    var cleanResult = Git.Execute(clean);

                    return new StatusCollection(
                        statusResult.Concat(cleanResult.Select(name => new Status(System.IO.Path.Combine(BaseDirectory, name), FileStatus.Ignored)))
                    );
                }
            );    
        }

        ~RepositoryService()
        { DisposeObject(false); }

        public void Dispose()
        {
            DisposeObject(true);
            GC.SuppressFinalize(this);
        }

        private void DisposeObject(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                DisposeWorkingTree();

            _disposed = true;
        }

        public void OpenRepositoryAt(Path path)
        {
            DisposeWorkingTree();

            var repositoryRoot = FindTopLevelDirectory(path);

            Git = new GitApplication(repositoryRoot ?? path);
            RepositoryDirectory = BaseDirectory.Combine(".git");
            IsGitRepository = !ReferenceEquals(repositoryRoot, null);

            if (!IsGitRepository)
                Log.Info("Directory {0} is not in a Git working tree", path);
            else
            {
                Log.Debug("Found Git repository at {0}", repositoryRoot);

                _repositoryMonitor = new RepositoryMonitor(repositoryRoot);
                _repositoryMonitor.RepositoryChanged += OnRepositoryChanged;
                _repositoryMonitor.WorkingTreeChanged += OnWorkingTreeChanged;
            }

            // Notify everyone that the repository has changed
            OnRepositoryChanged(null);
        }

        public void CloseRepository()
        {
            DisposeWorkingTree();
            Git = new GitApplication(null);
            IsGitRepository = false;
            OnRepositoryChanged(null);
        }

        public void ResetChanges(IEnumerable<Path> files)
        {
            var command = new Checkout
            {
                FilePaths = files.Select(path => path.GetRelativeTo(BaseDirectory).ActualPath)
            };

            Git.Execute(command);
        }

        public void DisplayUnstagedChangesAsync(Path file)
        {
            var command = new Diff
            {
                ViewInTool = true, 
                FilePath = file.GetRelativeTo(BaseDirectory).ActualPath
            };

            Git.ExecuteAsync(command);
        }

        private void DisposeWorkingTree()
        {
            if (_repositoryMonitor != null)
            {
                _repositoryMonitor.Dispose();
                _repositoryMonitor = null;
            }
        }

        private void OnWorkingTreeChanged(HashSet<Path> obj)
        {
            if (_status.IsValid)
            {
                // Ignore changes to ignored files
                // Note that accessing the status here will not cause it to be rehydrated as it is already valid
                if (obj != null)
                {
                    var ignored = _status.Value.Statuses.Where(s => (s.FileStatus & FileStatus.Ignored) != 0);
                    obj.ExceptWith(ignored.Select(s => s.FilePath));

                    // If the changes were just to ignored files, then do not report anything
                    if (obj.Count == 0)
                        return;    
                }

                _status.Invalidate();
                _workingTreeChangedEvent.Publish(this);
            }
        }

        private void OnRepositoryChanged(HashSet<Path> hashSet)
        {
            _repositoryChangedEvent.Publish(hashSet);
            OnWorkingTreeChanged(hashSet);
        }

        private Path FindTopLevelDirectory(string directory)
        {
            try
            {
                var app = new GitApplication(directory);
                return app.Execute(new RevParse());
            }
            catch
            {
                return null;
            }
        }
    }
}