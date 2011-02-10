using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using GitWorkflows.Common;
using GitWorkflows.Git;
using GitWorkflows.Services;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;

namespace GitWorkflows.Package
{
    [Guid(Constants.guidSccProviderServiceString)]
    [Export]
    public class SourceControlProvider : IVsSccProvider, IVsSccManager2
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(SourceControlProvider).FullName);

        [Import]
        private IRepositoryService _repositoryService;

        private bool _active;

        public event EventHandler Activated;
        public event EventHandler Deactivated;

        [ImportingConstructor]
        internal SourceControlProvider(IServiceProvider serviceLocator, IServiceContainer serviceContainer)
        {
            // Register the provider with the source control manager
            // If the package is to become active, this will also callback on OnActiveStateChange and the menu commands will be enabled
            var rscp = serviceLocator.GetService<IVsRegisterScciProvider>();
            rscp.RegisterSourceControlProvider(Constants.guidSccProvider);

            serviceContainer.AddService(GetType(), this, true);
        }

        // Called by the scc manager when the provider is activated. 
        // Make visible and enable if necessary scc related menu commands
        public int SetActive()
        {
            Log.Debug("Provider activated");

            _active = true;

            var handler = Activated;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return VSConstants.S_OK;
        }

        // Called by the scc manager when the provider is deactivated. 
        // Hides and disable scc related menu commands
        public int SetInactive()
        {
            Log.Debug("Provider deactivated");

            _active = false;

            var handler = Deactivated;
            if (handler != null)
                handler(this, EventArgs.Empty);

            return VSConstants.S_OK;
        }

        public int AnyItemsUnderSourceControl(out int pfResult)
        {
            pfResult = 0;
            return VSConstants.S_OK;
        }

        #region Implementation of IVsSccManager2

        public int RegisterSccProject(IVsSccProject2 pscp2Project, string pszSccProjectName, string pszSccAuxPath, string pszSccLocalPath, string pszProvider)
        {
            return VSConstants.S_OK;
        }

        public int UnregisterSccProject(IVsSccProject2 pscp2Project)
        {
            return VSConstants.S_OK;
        }

        public int GetSccGlyph(int cFiles, string[] rgpszFullPaths, VsStateIcon[] rgsiGlyphs, uint[] rgdwSccStatus)
        {
            if (!_active)
            {
                rgsiGlyphs.Fill(VsStateIcon.STATEICON_NOSTATEICON);
                if (rgdwSccStatus != null)
                    rgdwSccStatus.Fill((uint)__SccStatus.SCC_STATUS_NOTCONTROLLED);

                return VSConstants.S_OK;
            }

            // TODO: Set the status to something more meaningful
            if (rgdwSccStatus != null)
                rgdwSccStatus[0] = (uint)(_active ? __SccStatus.SCC_STATUS_CONTROLLED : __SccStatus.SCC_STATUS_NOTCONTROLLED);

            var status = _repositoryService.Status.GetStatusOf(rgpszFullPaths[0]);
            var fileStatus = status == null ? FileStatus.NotModified : status.FileStatus;
            
            // TODO: Handle status combinations properly
            // For now, if a file status is a combination of modified and something else, 
            // we treat it as modified
            if ((fileStatus & FileStatus.Modified) != 0)
                fileStatus = FileStatus.Modified;

            switch (fileStatus)
            {
                case FileStatus.Untracked:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_BLANK;
                    break;

                case FileStatus.NotModified:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_CHECKEDIN;
                    break;

                case FileStatus.Added:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_EDITABLE;
                    break;

                case FileStatus.Modified:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_CHECKEDOUT;
                    break;

                case FileStatus.Ignored:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_EXCLUDEDFROMSCC;
                    break;

                case FileStatus.Removed:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_ORPHANED;
                    break;

                case FileStatus.Conflicted:
                    rgsiGlyphs[0] = VsStateIcon.STATEICON_DISABLED;
                    break;
                    
                default:
                    if (rgdwSccStatus != null)
                        rgdwSccStatus[0] = (uint)__SccStatus.SCC_STATUS_NOTCONTROLLED;

                    rgsiGlyphs[0] = VsStateIcon.STATEICON_NOSTATEICON;
                    break;
            }

            return VSConstants.S_OK;
        }

        public int GetSccGlyphFromStatus(uint dwSccStatus, VsStateIcon[] psiGlyph)
        {
            psiGlyph[0] = VsStateIcon.STATEICON_BLANK;
            return VSConstants.S_OK;
        }

        public int IsInstalled(out int pbInstalled)
        {
            // All source control packages should always return S_OK and set pbInstalled to nonzero
            pbInstalled = 1;
            return VSConstants.S_OK;
        }

        public int BrowseForProject(out string pbstrDirectory, out int pfOK)
        {
            pbstrDirectory = null;
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }

        public int CancelAfterBrowseForProject()
        {
            return VSConstants.E_NOTIMPL;
        }

        #endregion
    }
}