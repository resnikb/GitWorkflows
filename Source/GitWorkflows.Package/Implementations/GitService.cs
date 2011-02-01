using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using GitWorkflows.Package.Common;
using GitWorkflows.Package.Git;
using GitWorkflows.Package.Git.Commands;
using GitWorkflows.Package.Interfaces;
using NLog;
using Path = GitWorkflows.Package.FileSystem.Path;
using Status = GitWorkflows.Package.Git.Status;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(IGitService))]
    class GitService : IGitService, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(GitService).FullName);

        private readonly Cache<Status> _status;
        private bool _disposed;
        private DateTime _lastChangeTime = DateTime.MinValue;
        private Path _gitRoot;
        private FileSystemWatcher _watcher;

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
            _status = new Cache<Status>(
                () =>
                {
                    var clean = new Clean
                    {
                        Target = Clean.CleanTarget.Ignored,
                        IncludeDirectories = false
                    };

                    var statusResult = Git.Execute(new Git.Commands.Status());
                    var cleanResult = Git.Execute(clean);

                    return new Status(
                        statusResult.Concat(cleanResult.Select(name => new KeyValuePair<FileStatus, string>(FileStatus.Ignored, name))),
                        Git.WorkingDirectory
                    );
                }
            );    
        
            solutionService.SolutionClosed += DisposeWorkingTree;
            solutionService.SolutionOpening += OnSolutionOpening;
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

        public FileStatus GetStatusOf(string path)
        { return _status.Value.GetStatusOf(path); }

        private void OnSolutionOpening(Path solutionPath)
        {
            var directory = solutionPath.DirectoryName;
            Log.Debug("Loading solution from directory: {0}", directory);

            RepositoryRoot = WorkingTree.FindTopLevelDirectory(directory);
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

                _lastChangeTime = DateTime.MinValue;
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

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        { OnFileChanged(sender, e); }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            var changeInterval = DateTime.Now.Subtract(_lastChangeTime);
            if (changeInterval.TotalSeconds < 0.5)
                return;

            var path = new Path(e.FullPath);

            // Ignore changes to directories
            if (path.IsDirectory)
                return;

            if (_gitRoot.IsParentOf(path))
                OnRepositoryChanged();
            else if (!path.GetCanonicalComponents().Any(c => c.StartsWith("_resharper.")))
                OnWorkingTreeChanged();

            _lastChangeTime = DateTime.Now;
        }

        private void DisposeWorkingTree()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnRepositoryChanged()
        {
            _status.Invalidate();

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