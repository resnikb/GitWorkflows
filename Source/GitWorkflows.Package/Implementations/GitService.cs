using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using GitWorkflows.Common;
using GitWorkflows.Git;
using GitWorkflows.Git.Commands;
using GitWorkflows.Package.Interfaces;
using NLog;
using Path = GitWorkflows.Common.Path;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(IGitService))]
    class GitService : IGitService, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(GitService).FullName);

        private readonly CachedValue<StatusCollection> _status;
        private bool _disposed;
        private Path _gitRoot;
        private FileSystemWatcher _watcher;
        private readonly Timer _timer;
        private readonly HashSet<Path> _changedRepositoryFiles = new HashSet<Path>();
        private readonly HashSet<Path> _changedWorkingTreeFiles = new HashSet<Path>();

        public event EventHandler RepositoryChanged;
        public event EventHandler WorkingTreeChanged;

        public GitApplication Git
        { get; private set; }

        public Path RepositoryRoot
        { get; private set; }

        public bool IsRepositoryOpen
        {
            get { return !ReferenceEquals(RepositoryRoot, null); }
        }

        [ImportingConstructor]
        public GitService(ISolutionService solutionService)
        {
            _status = new CachedValue<StatusCollection>(
                () =>
                {
                    var clean = new Clean
                    {
                        Target = Clean.CleanTarget.Ignored,
                        IncludeDirectories = false
                    };

                    var statusResult = Git.Execute(new Git.Commands.Status());
                    var cleanResult = Git.Execute(clean);

                    return new StatusCollection(
                        statusResult.Concat(cleanResult.Select(name => new Git.Status(System.IO.Path.Combine(Git.WorkingDirectory, name), FileStatus.Ignored)))
                    );
                }
            );    
        
            solutionService.SolutionClosed += DisposeWorkingTree;
            solutionService.SolutionOpening += OnSolutionOpening;

            _timer = new Timer(OnChangeTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        ~GitService()
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
                // Dispose managed resources.
                DisposeWorkingTree();
            }

            // Dispose unmanaged resources.
            _disposed = true;
        }

        public Git.Status GetStatusOf(string path)
        { return _status.Value.GetStatusOf(path); }

        private void OnChangeTimer(object state)
        {
            lock (_changedRepositoryFiles)
            lock (_changedWorkingTreeFiles)
            {
                if (_changedRepositoryFiles.Count > 0)
                {
                    OnRepositoryChanged();
                    _changedRepositoryFiles.Clear();
                }

                if (_changedWorkingTreeFiles.Count > 0)
                {
                    OnWorkingTreeChanged();
                    _changedWorkingTreeFiles.Clear();
                }
            }
        }

        private void OnSolutionOpening(Path solutionPath)
        {
            var directory = solutionPath.DirectoryName;
            Log.Debug("Loading solution from directory: {0}", directory);

            RepositoryRoot = FindTopLevelDirectory(directory);
            if ( ReferenceEquals(RepositoryRoot, null) )
            {
                Log.Info("Directory is not in a Git working tree");
                Git = new GitApplication(directory);
            }
            else
            {
                Git = new GitApplication(RepositoryRoot);
                Log.Debug("Found Git repository at {0}", RepositoryRoot);

                _gitRoot = RepositoryRoot.Combine(".git");

                _watcher = new FileSystemWatcher
                {
                    Path = RepositoryRoot,
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.LastWrite
                };
                _watcher.Created += OnFileChanged;
                _watcher.Deleted += OnFileChanged;
                _watcher.Changed += OnFileChanged;
                _watcher.Renamed += OnFileRenamed;
                _watcher.EnableRaisingEvents = true;
            }
        }

        private Path FindTopLevelDirectory(string directory)
        {
            try
            {
                var app = new GitApplication(directory);
                return app.Execute(new RevParse(RevParse.PropertyOption.TopLevelDirectory));
            }
            catch
            {
                return null;
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        { OnFileChanged(sender, e); }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var path = new Path(e.FullPath);
            if (path.IsDirectory)
                return;

            if (_gitRoot.IsParentOf(path))
            {
                if (path.HasExtension && path.Extension.ToLowerInvariant() == ".lock")
                    return;

                lock (_changedRepositoryFiles)
                    _changedRepositoryFiles.Add(path);
                
                _timer.Change(500, 1000);
            }
            else if (!path.GetCanonicalComponents().Any(c => c.StartsWith("_resharper.")))
            {
                lock (_changedWorkingTreeFiles)
                    _changedWorkingTreeFiles.Add(e.FullPath);
                
                _timer.Change(500, 1000);
            }
        }

        private void DisposeWorkingTree()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnRepositoryChanged()
        {
            var handler = RepositoryChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void OnWorkingTreeChanged()
        {
            _status.Invalidate();

            var handler = WorkingTreeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}