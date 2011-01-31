using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using GitWorkflows.Package.Common;
using GitWorkflows.Package.Extensions;
using GitWorkflows.Package.FileSystem;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;

namespace GitWorkflows.Package.Git
{
    [Export(typeof(IGitService))]
    [Export(typeof(IBranchManager))]
    class GitService : IGitService, IVsFileChangeEvents, IVsSolutionEvents, IVsSolutionLoadEvents, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(GitService).FullName);

        private readonly IServiceProvider _serviceLocator;
        private readonly IVsSolution _vsSolution;
        private readonly uint _cookieSolutionEvents;
        private uint _cookieDirChange;
        private bool _disposed;
        private GitApplication _git;

        public event EventHandler SolutionChanged;
        public event EventHandler WorkingTreeChanged;
        public event EventHandler RepositoryChanged;

        public bool IsRefreshEnabled
        { get; private set; }

        private readonly Cache<Status> _status;

        public bool IsRepositoryOpen
        { get; private set; }

        public FileStatus GetStatusOf(string path)
        { return _status.Value.GetStatusOf(path); }

        public IEnumerable<IVsSccProject2> ControllableProjects
        {
            get
            {
                var rguidEnumOnlyThisType = Guid.Empty;
                IEnumHierarchies ppenum;
                if ( ErrorHandler.Failed(_vsSolution.GetProjectEnum((uint)__VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref rguidEnumOnlyThisType, out ppenum)) )
                    yield break;

                var rgelt = new IVsHierarchy[1];
                uint pceltFetched;
                while (ppenum.Next(1, rgelt, out pceltFetched) == VSConstants.S_OK && pceltFetched == 1)
                {
                    var sccProject2 = rgelt[0] as IVsSccProject2;
                    if (sccProject2 != null)
                        yield return sccProject2;
                }
            }
        }

        private void Reload()
        {
            string directory, fileName, userFile;
            ErrorHandler.ThrowOnFailure(_vsSolution.GetSolutionInfo(out directory, out fileName, out userFile));

            ErrorHandler.ThrowOnFailure(_vsSolution.CloseSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_PromptSave, null, 0));
            ErrorHandler.ThrowOnFailure(_vsSolution.OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, fileName));
        }

        private void SaveAllDocuments()
        {
            var docService = _serviceLocator.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
            docService.SaveDocuments((uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_SaveIfDirty, null, 0, 0);
        }

        private void RefreshSourceControlGlyphs(IEnumerable<VSITEMSELECTION> nodes = null)
        {
            if (!IsRefreshEnabled)
                return;

            IsRefreshEnabled = false;
            try
            {
                if (nodes == null)
                {
                    var nodeList = ControllableProjects.Cast<IVsHierarchy>()
                                                       .Select(p => new VSITEMSELECTION { itemid = VSConstants.VSITEMID_ROOT, pHier = p })
                                                       .ToList();
                    nodeList.Add(new VSITEMSELECTION { itemid = VSConstants.VSITEMID_ROOT, pHier = (IVsHierarchy)_vsSolution });
                    nodes = nodeList;
                }

                nodes.ForEach(RefreshSourceControlGlyphs);
            }
            finally
            {
                IsRefreshEnabled = true;
            }
        }

        private void RefreshSourceControlGlyphs(VSITEMSELECTION node)
        {
            var project = node.pHier as IVsSccProject2;
            if (project != null)
            {
                // Refresh all the glyphs in the project; the project will call back GetSccGlyphs() 
                // with the files for each node that will need new glyph
                project.SccGlyphChanged(0, null, null, null);
            }
            else if (node.itemid == VSConstants.VSITEMID_ROOT)
            {
                // Note: The solution's hierarchy does not implement IVsSccProject2, IVsSccProject interfaces
                // It may be a pain to treat the solution as special case everywhere; a possible workaround is 
                // to implement a solution-wrapper class, that will implement IVsSccProject2, IVsSccProject and
                // IVsHierarhcy interfaces, and that could be used in provider's code wherever a solution is needed.
                // This approach could unify the treatment of solution and projects in the provider's code.

                // Until then, solution is treated as special case
                var sccService = _serviceLocator.GetService<SourceControlProvider>();

                string directory, fileName, userFile;
                ErrorHandler.ThrowOnFailure(_vsSolution.GetSolutionInfo(out directory, out fileName, out userFile));

                var rgpszFullPaths = new[] {fileName};
                var rgsiGlyphs = new VsStateIcon[1];
                sccService.GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, new uint[1]);

                // Set the solution's glyph directly in the hierarchy
                ((IVsHierarchy)_vsSolution).SetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_StateIconIndex, rgsiGlyphs[0]);
            }
        }

        [ImportingConstructor]
        public GitService(IServiceProvider serviceLocator)
        {
            IsRefreshEnabled = true;

            _currentBranch = new Cache<Branch>(
                () => new Branch(_git.Execute(new Commands.SymbolicRef {Name="HEAD"}))
            );
            
            _branches = new Cache<Branch[]>(
                () => _git.Execute(new Commands.GetBranches()).Select(name => new Branch(name)).ToArray()
            );

            _status = new Cache<Status>(
                () =>
                {
                    var clean = new Commands.Clean
                    {
                        Target = Commands.Clean.CleanTarget.Ignored,
                        IncludeDirectories = false
                    };

                    var taskStatus = Task.Factory.StartNew(() => _git.Execute(new Commands.Status()));
                    var taskClean = Task.Factory.StartNew(() => _git.Execute(clean));

                    return new Status(
                        taskStatus.Result.Concat(taskClean.Result.Select(name => new KeyValuePair<FileStatus, string>(FileStatus.Ignored, name))),
                        _git.WorkingDirectory
                    );
                }
            );    
        
            _serviceLocator = serviceLocator;
            _vsSolution = _serviceLocator.GetService<SVsSolution, IVsSolution>();
            _vsSolution.AdviseSolutionEvents(this, out _cookieSolutionEvents);
        }

        private void RaiseSolutionChanged(EventArgs e)
        {
            var handler = SolutionChanged;
            if (handler != null)
                handler(this, e);
        }

        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        { return VSConstants.S_OK; }

        public int DirectoryChanged(string pszDirectory)
        {
            if (!IsRefreshEnabled)
                return VSConstants.S_OK;

            if (!new Path(_git.WorkingDirectory).IsParentOf(pszDirectory))
                RaiseWorkingTreeChanged();
            else
                RaiseRepositoryChanged();

            RefreshSourceControlGlyphs();
            return VSConstants.S_OK;
        }

        private void RaiseWorkingTreeChanged()
        {
            var handler = WorkingTreeChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void RaiseRepositoryChanged()
        {
            var handler = RepositoryChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #region Implementation of IVsSolutionLoadEvents

        /// <summary>
        /// Fired before a solution open begins. Extenders can activate a solution load manager by
        /// setting 
        /// <see cref="F:Microsoft.VisualStudio.Shell.Interop.__VSPROPID4.VSPROPID_ActiveSolutionLoadManager"/>
        /// .
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns 
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an
        /// error code.
        /// </returns>
        /// <param name="pszSolutionFilename">The name of the solution file.</param>
        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            DisposeWorkingTree();

            var directory = System.IO.Path.GetDirectoryName(pszSolutionFilename);
            Log.Debug("Loading solution from directory: {0}", directory);

            var repositoryRoot = WorkingTree.FindTopLevelDirectory(directory);
            if ( ReferenceEquals(repositoryRoot, null) )
            {
                Log.Info("Directory is not in a Git working tree");
                _git = new GitApplication(directory);
            }
            else
            {
                IsRepositoryOpen = true;
                _git = new GitApplication(repositoryRoot);
                Log.Debug("Found Git repository at {0}", repositoryRoot);

                var fileChangeService = _serviceLocator.GetService<SVsFileChangeEx, IVsFileChangeEx>();
                fileChangeService.AdviseDirChange(repositoryRoot, 1, this, out _cookieDirChange);
            }

            RaiseSolutionChanged(EventArgs.Empty);
            return VSConstants.S_OK;    
        }

        private void DisposeWorkingTree()
        {
            IsRepositoryOpen = false;
            if (_cookieDirChange != VSConstants.VSCOOKIE_NIL)
            {
                var fileChangeService = _serviceLocator.GetService<SVsFileChangeEx, IVsFileChangeEx>();
                fileChangeService.UnadviseDirChange(_cookieDirChange);
                _cookieDirChange = VSConstants.VSCOOKIE_NIL;
            }
        }

        /// <summary>
        /// Fired when background loading of projects is beginning again after the initial solution
        /// open operation has completed. 
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns 
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an
        /// error code.
        /// </returns>
        public int OnBeforeBackgroundSolutionLoadBegins()
        { return VSConstants.S_OK; }

        /// <summary>
        /// Fired before background loading a batch of projects. Normally a background batch loads a
        /// single pending project. This is a cancelable event.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns 
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an
        /// error code.
        /// </returns>
        /// <param name="pfShouldDelayLoadToNextIdle">[out] true if other background operations
        /// should complete before starting to load the project, otherwise false.</param>
        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Fired when loading a batch of dependent projects as part of loading a solution in the
        /// background. 
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns 
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an
        /// error code.
        /// </returns>
        /// <param name="fIsBackgroundIdleBatch">true if the batch is loaded in the background,
        /// otherwise false.</param>
        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        { return VSConstants.S_OK; }

        /// <summary>
        /// Fired when the loading of a batch of dependent projects is complete.
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns 
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an
        /// error code.
        /// </returns>
        /// <param name="fIsBackgroundIdleBatch">true if the batch is loaded in the background,
        /// otherwise false.</param>
        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        { return VSConstants.S_OK; }

        /// <summary>
        /// Fired when the solution load process is fully complete, including all background loading
        /// of projects. 
        /// </summary>
        /// <returns>
        /// If the method succeeds, it returns 
        /// <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an
        /// error code.
        /// </returns>
        public int OnAfterBackgroundSolutionLoadComplete()
        { return VSConstants.S_OK; }

        #endregion

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
                var solution = _serviceLocator.GetService<SVsSolution, IVsSolution>();
                solution.UnadviseSolutionEvents(_cookieSolutionEvents);
            }

            // Dispose unmanaged resources.
            _disposed = true;
        }

        #region Implementation of IVsSolutionEvents

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        { return VSConstants.S_OK; }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        { return VSConstants.S_OK; }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        { return VSConstants.S_OK; }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        { return VSConstants.S_OK; }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            if (IsRepositoryOpen)
                RefreshSourceControlGlyphs();

            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnBeforeCloseSolution(object pUnkReserved)
        { return VSConstants.S_OK; }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            DisposeWorkingTree();
            RaiseSolutionChanged(EventArgs.Empty);
            return VSConstants.S_OK;
        }

        #endregion

        #region Implementation of IBranchManager

        private readonly Cache<Branch> _currentBranch;
        private readonly Cache<Branch[]> _branches;

        public Branch CurrentBranch
        {
            get { return _currentBranch.Value; }
        }

        public IEnumerable<Branch> Branches
        {
            get { return _branches.Value; }
        }

        public Branch Checkout(string name, bool force)
        {
            var command = new Commands.Checkout {Name = name, Force = force};
            _currentBranch.Invalidate();
            _git.Execute(command);
            Reload();
            return CurrentBranch;
        }

        public Branch Create(string name, bool checkout)
        {
            _branches.Invalidate();
            if (checkout)
            {
                var checkoutCommand = new Commands.Checkout { CreateBranch = true, Name = name };
                _currentBranch.Invalidate();
                _git.Execute(checkoutCommand);
                Reload();
                return CurrentBranch;
            }

            var branchCommand = new Commands.Branch {Name = name};
            _git.Execute(branchCommand);
            return new Branch(name);
        }

        #endregion
    }
}