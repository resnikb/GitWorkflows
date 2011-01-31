using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Package.Common;
using GitWorkflows.Package.Extensions;
using GitWorkflows.Package.FileSystem;
using GitWorkflows.Package.Git;
using GitWorkflows.Package.Git.Commands;
using GitWorkflows.Package.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using Status = GitWorkflows.Package.Git.Status;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(IGitService))]
    class GitService : IGitService, IVsFileChangeEvents, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(GitService).FullName);

        private readonly ISolutionService _solutionService;
        private readonly IServiceProvider _serviceProvider;
        private readonly Cache<Status> _status;
        private uint _cookieDirChange;
        private bool _disposed;

        public GitApplication Git
        { get; private set; }

        public Path RepositoryRoot
        { get; private set; }

        public bool IsRepositoryOpen
        {
            get { return !ReferenceEquals(RepositoryRoot, null); }
        }

        [ImportingConstructor]
        public GitService(IServiceProvider serviceProvider, ISolutionService solutionService)
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
        
            _serviceProvider = serviceProvider;
            _solutionService = solutionService;
            _solutionService.SolutionClosed += DisposeWorkingTree;
            _solutionService.SolutionOpening += OnSolutionOpening;
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

                var fileChangeService = _serviceProvider.GetService<SVsFileChangeEx, IVsFileChangeEx>();
                fileChangeService.AdviseDirChange(RepositoryRoot, 1, this, out _cookieDirChange);
            }
        }

        private void DisposeWorkingTree()
        {
            if (_cookieDirChange != VSConstants.VSCOOKIE_NIL)
            {
                var fileChangeService = _serviceProvider.GetService<SVsFileChangeEx, IVsFileChangeEx>();
                fileChangeService.UnadviseDirChange(_cookieDirChange);
                _cookieDirChange = VSConstants.VSCOOKIE_NIL;
            }
        }

        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        { return VSConstants.S_OK; }

        public int DirectoryChanged(string pszDirectory)
        {
            _solutionService.RefreshSourceControlIcons();
            return VSConstants.S_OK;
        }
    }
}