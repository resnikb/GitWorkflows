using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Git;
using GitWorkflows.Git.Commands;
using GitWorkflows.Services.Events;
using NLog;
using Path = GitWorkflows.Common.Path;
using Status = GitWorkflows.Git.Status;

namespace GitWorkflows.Services.Implementations
{
    [Export(typeof(IRepositoryService))]
    public class RepositoryService : IRepositoryService, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(RepositoryService).FullName);
        
        private readonly GitRepositoryChangedEvent _repositoryChangedEvent;
        private readonly GitWorkingTreeChangedEvent _workingTreeChangedEvent;
        private readonly CachedValue<StatusCollection> _status;
        private FileSystemWatcher _watcher;
        private IDisposable _subscription;
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

        [ImportingConstructor]
        public RepositoryService(
            GitRepositoryChangedEvent repositoryChangedEvent, 
            GitWorkingTreeChangedEvent workingTreeChangedEvent
        )
        {
            _repositoryChangedEvent = repositoryChangedEvent;
            _workingTreeChangedEvent = workingTreeChangedEvent;
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
                DisposeWatcher();

            _disposed = true;
        }

        public void OpenRepositoryAt(Path path)
        {
            DisposeWatcher();

            var repositoryRoot = FindTopLevelDirectory(path);

            Git = new GitApplication(repositoryRoot ?? path);
            RepositoryDirectory = BaseDirectory.Combine(".git");
            IsGitRepository = !ReferenceEquals(repositoryRoot, null);

            if (!IsGitRepository)
                Log.Info("Directory {0} is not in a Git working tree", path);
            else
            {
                Log.Debug("Found Git repository at {0}", repositoryRoot);
                CreateFileWatcher();
            }

            // Notify everyone that the repository has changed
            OnRepositoryChanged(null);
        }

        private void CreateFileWatcher()
        {
            _watcher = new FileSystemWatcher
            {
                Path = BaseDirectory,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.LastWrite,
            };

            var changeEvents = Observable.FromEvent<FileSystemEventArgs>(e => _watcher.Changed += new FileSystemEventHandler(e), e => _watcher.Changed -= new FileSystemEventHandler(e));
            var createEvents = Observable.FromEvent<FileSystemEventArgs>(e => _watcher.Created += new FileSystemEventHandler(e), e => _watcher.Created -= new FileSystemEventHandler(e));
            var deleteEvents = Observable.FromEvent<FileSystemEventArgs>(e => _watcher.Deleted += new FileSystemEventHandler(e), e => _watcher.Deleted -= new FileSystemEventHandler(e));
            var renameEvents = Observable.FromEvent<FileSystemEventArgs>(e => _watcher.Renamed += new RenamedEventHandler(e), e => _watcher.Renamed -= new RenamedEventHandler(e));

            _subscription = Observable.Merge(changeEvents, createEvents, deleteEvents, renameEvents) 
                .Select(e => e.EventArgs.FullPath)
                .BufferWithTime(TimeSpan.FromSeconds(1))
                .Subscribe(
                    changes =>
                    {
                        var changeList = new List<string>(256);
                        changes.Subscribe(
                            changeList.Add, 
                            () => 
                            {
                                if (changeList.Count > 0)
                                    OnChangesDetected(changeList);
                            }
                        );
                    }
                );

            _watcher.EnableRaisingEvents = true;
        }

        public void CloseRepository()
        {
            DisposeWatcher();
            Git = new GitApplication(null);
            IsGitRepository = false;
            OnRepositoryChanged(null);
        }

        public void ResetChanges(IEnumerable<Path> files)
        {
            var statuses = files.Select(_status.Value.GetStatusOf)
                                .GroupBy(s => s.FileStatus != FileStatus.Untracked && s.FileStatus != FileStatus.Ignored)
                                .ToList();

            if (statuses.Any(g => g.Key))
            {
                var command = new Checkout
                {
                    FilePaths = statuses.First(g => g.Key).Select(s => s.FilePath.GetRelativeTo(BaseDirectory).ActualPath)
                };
                Git.Execute(command);
            }

            if (statuses.Any(g => !g.Key))
            {
                statuses.First(g => !g.Key).ForEach(
                    s =>
                    { 
                        try
                        {
                            File.Delete(s.FilePath);
                        }
                        catch
                        {
                        
                        }
                    }
                );
            }
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

        private void DisposeWatcher()
        {
            if (_watcher != null)
            {
                using (_watcher)
                using (_subscription)
                {
                    _watcher.EnableRaisingEvents = false;
                }

                _watcher = null;
            }
        }

        private void OnChangesDetected(IEnumerable<string> changes)
        {
            var repositoryChanges = new HashSet<Path>();
            var treeChanges = new HashSet<Path>();

            foreach (Path path in changes)
            {
                if (path.IsDirectory)
                    continue;

                if (!RepositoryDirectory.IsParentOf(path))
                    treeChanges.Add(path);
                else if (!path.HasExtension || path.Extension.ToLowerInvariant() != ".lock")
                    repositoryChanges.Add(path);
            }

            if (repositoryChanges.Count > 0)
                OnRepositoryChanged(repositoryChanges);

            if (treeChanges.Count > 0)
                OnWorkingTreeChanged(treeChanges);
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
            OnWorkingTreeChanged(null);
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