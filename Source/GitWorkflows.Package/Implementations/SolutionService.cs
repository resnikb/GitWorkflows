using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(ISolutionService))]
    class SolutionService : ISolutionService, IVsSolutionEvents, IVsSolutionLoadEvents, IDisposable
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(SolutionService).FullName);

        [Import]
        private IRepositoryService _repositoryService;

        private readonly IVsSolution _vsSolution;
        private readonly IServiceProvider _serviceProvider;
        private uint _cookieSolutionEvents;
        private bool _isRefreshEnabled;
        private bool _disposed;
        private bool _isReloading;

        [ImportingConstructor]
        public SolutionService(IServiceProvider serviceProvider, SourceControlProvider sourceControlProvider)
        {
            _serviceProvider = serviceProvider;
            _vsSolution = serviceProvider.GetService<SVsSolution, IVsSolution>();
            _isRefreshEnabled = true;

            sourceControlProvider.Activated += (sender, e) =>
            {
                string directory, fileName, userFile;
                _vsSolution.GetSolutionInfo(out directory, out fileName, out userFile);

                _vsSolution.AdviseSolutionEvents(this, out _cookieSolutionEvents);
                if (string.IsNullOrEmpty(directory))
                    _repositoryService.CloseRepository();
                else
                    _repositoryService.OpenRepositoryAt(directory);
            };

            sourceControlProvider.Deactivated += (sender, e) => 
            {
                StopListeningToSolutionEvents();
                _repositoryService.CloseRepository();
            };
        }

        public void Reload()
        {
            if (_isReloading)
                return;

            _isReloading = true;

            try
            {
                string directory, fileName, userFile;
                if (ErrorHandler.Failed(_vsSolution.GetSolutionInfo(out directory, out fileName, out userFile)) || string.IsNullOrEmpty(fileName))
                    return;

                ErrorHandler.ThrowOnFailure(_vsSolution.CloseSolutionElement((uint)__VSSLNSAVEOPTIONS.SLNSAVEOPT_PromptSave, null, 0));
                ErrorHandler.ThrowOnFailure(_vsSolution.OpenSolutionFile((uint)__VSSLNOPENOPTIONS.SLNOPENOPT_Silent, fileName));
            }
            finally
            {
                _isReloading = false;
            }
        }

        public string GetNameOfParentProject(Path path)
        {
            var actualPath = path.ActualPath;
            var prio = new VSDOCUMENTPRIORITY[1];
            foreach (var project in GetControllableProjects().OfType<IVsProject2>())
            {
                int pfFound;
                uint itemid;
                if ( ErrorHandler.Succeeded(project.IsDocumentInProject(actualPath, out pfFound, prio, out itemid)) && pfFound != 0 )
                {
                    object name;
                    ((IVsHierarchy)project).GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_Name, out name);
                    return (string)name;
                }
            }

            return null;
        }

        public void RefreshSourceControlIcons()
        {
            if (!_isRefreshEnabled)
                return;

            _isRefreshEnabled = false;
            try
            {
                var nodeList = GetControllableProjects()
                                    .Cast<IVsHierarchy>()
                                    .Select(p => new VSITEMSELECTION { itemid = VSConstants.VSITEMID_ROOT, pHier = p })
                                    .ToList();

                nodeList.Add(new VSITEMSELECTION { itemid = VSConstants.VSITEMID_ROOT, pHier = (IVsHierarchy)_vsSolution });
                nodeList.ForEach(RefreshSourceControlGlyphs);
            }
            finally
            {
                _isRefreshEnabled = true;
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
                var sccService = _serviceProvider.GetService<SourceControlProvider>();

                string directory, fileName, userFile;
                ErrorHandler.ThrowOnFailure(_vsSolution.GetSolutionInfo(out directory, out fileName, out userFile));

                var rgpszFullPaths = new[] {fileName};
                var rgsiGlyphs = new VsStateIcon[1];
                sccService.GetSccGlyph(1, rgpszFullPaths, rgsiGlyphs, new uint[1]);

                // Set the solution's glyph directly in the hierarchy
                ((IVsHierarchy)_vsSolution).SetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_StateIconIndex, rgsiGlyphs[0]);
            }
        }

        private IEnumerable<IVsSccProject2> GetControllableProjects()
        {
            var rguidEnumOnlyThisType = Guid.Empty;
            IEnumHierarchies ppenum;
            if (ErrorHandler.Failed(_vsSolution.GetProjectEnum((uint) __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION, ref rguidEnumOnlyThisType, out ppenum)))
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

        private void SaveAllDocuments()
        {
            var docService = _serviceProvider.GetService<SVsRunningDocumentTable, IVsRunningDocumentTable>();
            docService.SaveDocuments((uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_SaveIfDirty, null, 0, 0);
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
        { return VSConstants.S_OK; }

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
            _isRefreshEnabled = true;

            if (!_isReloading)
            { 
                string directory, fileName, userFile;
                _vsSolution.GetSolutionInfo(out directory, out fileName, out userFile);
                _repositoryService.OpenRepositoryAt(directory);
            }

            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        { return VSConstants.S_OK; }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            _isRefreshEnabled = false;
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            if (!_isReloading)
                _repositoryService.CloseRepository();

            return VSConstants.S_OK;
        }

        #endregion

        ~SolutionService()
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
                StopListeningToSolutionEvents();

            _disposed = true;
        }

        private void StopListeningToSolutionEvents()
        {
            if (_cookieSolutionEvents != VSConstants.VSCOOKIE_NIL)
            {
                _vsSolution.UnadviseSolutionEvents(_cookieSolutionEvents);
                _cookieSolutionEvents = VSConstants.VSCOOKIE_NIL;
            }
        }
    }
}