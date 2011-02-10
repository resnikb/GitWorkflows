using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Git.Commands;
using NLog;

namespace GitWorkflows.Git
{
    [Export(typeof(IRepositoryService))]
    public class RepositoryService : NotifyPropertyChanged, IRepositoryService, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(RepositoryService).FullName);
         
        private readonly CachedValue<StatusCollection> _status;
        private GitApplication _git;
        private bool _isGitRepository;
        private RepositoryMonitor _repositoryMonitor;
        private bool _disposed;
        private Path _repositoryDirectory;

        public event Action<HashSet<Path>> RepositoryChanged;

        public GitApplication Git
        {
            get { return _git; }
            private set { SetProperty(ref _git, value, () => Git); }
        }

        public StatusCollection Status
        {
            get { return _status.Value; }
        }

        public bool IsGitRepository
        {
            get { return _isGitRepository; }
            private set { SetProperty(ref _isGitRepository, value, () => IsGitRepository); }
        }

        public Path BaseDirectory
        {
            get { return Git.WorkingDirectory; }
        }

        public Path RepositoryDirectory
        {
            get { return _repositoryDirectory; }
            private set { SetProperty(ref _repositoryDirectory, value, () => RepositoryDirectory); }
        }

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

                    var statusResult = Git.Execute(new Commands.Status());
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
            _status.Invalidate();

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

            RaisePropertyChanged(() => Status);
            
            // Notify everyone that the repository has changed
            OnRepositoryChanged(null);
        }

        public void CloseRepository()
        {
            DisposeWorkingTree();
            _status.Invalidate();
            Git = new GitApplication(null);
            IsGitRepository = false;
            RaisePropertyChanged(() => Status);
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
                RaisePropertyChanged(() => Status);
            }
        }

        private void OnRepositoryChanged(HashSet<Path> hashSet)
        {
            var handler = RepositoryChanged;
            if (handler != null) 
                handler(hashSet);

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