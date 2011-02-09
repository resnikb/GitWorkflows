using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GitWorkflows.Git
{
    public class RepositoryMonitor : IDisposable
    {
        private readonly Common.Path _repositoryPath;
        private readonly FileSystemWatcher _watcher;
        private readonly IDisposable _subscription;
        private bool _disposed;

        public event Action<HashSet<Common.Path>> RepositoryChanged;
        public event Action<HashSet<Common.Path>> WorkingTreeChanged;

        public RepositoryMonitor(string rootPath)
        {
            _repositoryPath = Path.Combine(rootPath, ".git");

            _watcher = new FileSystemWatcher
            {
                Path = rootPath,
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
                                            });

            _watcher.EnableRaisingEvents = true;
        }

        private void OnChangesDetected(IEnumerable<string> changes)
        {
            var repositoryChanges = new HashSet<Common.Path>();
            var treeChanges = new HashSet<Common.Path>();

            foreach (var changeArgs in changes)
            {
                var path = new Common.Path(changeArgs);
                
                if (path.IsDirectory)
                    continue;

                if (_repositoryPath.IsParentOf(path))
                {
                    if (path.HasExtension && path.Extension.ToLowerInvariant() == ".lock")
                        continue;

                    repositoryChanges.Add(path);
                }
                else
                    treeChanges.Add(path);
            }

            if (repositoryChanges.Count > 0)
                RaiseRepositoryChanged(repositoryChanges);

            if (treeChanges.Count > 0)
                RaiseWorkingTreeChanged(treeChanges);
        }

        private void RaiseRepositoryChanged(HashSet<Common.Path> repositoryChanges)
        {
            var handler = RepositoryChanged;
            if (handler != null)
                handler(repositoryChanges);
        }

        private void RaiseWorkingTreeChanged(HashSet<Common.Path> treeChanges)
        {
            var handler = WorkingTreeChanged;
            if (handler != null)
                handler(treeChanges);
        }

        ~RepositoryMonitor()
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
            {
                using (_watcher)
                using (_subscription)
                {
                    _watcher.EnableRaisingEvents = false;
                }
            }

            _disposed = true;
        }
    }
}