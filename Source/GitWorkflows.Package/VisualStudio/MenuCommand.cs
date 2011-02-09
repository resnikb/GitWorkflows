using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using GitWorkflows.Common;
using GitWorkflows.Git;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;

namespace GitWorkflows.Package.VisualStudio
{
    abstract class MenuCommand : IPartImportsSatisfiedNotification
    {
        [Import]
        protected IServiceProvider ServiceProvider
        { get; private set; }

        [Import]
        protected SourceControlProvider SourceControlProvider
        { get; private set; }

        [Import]
        protected Microsoft.VisualStudio.Shell.Package HostPackage
        { get; private set; }

        [Import]
        protected IRepositoryService RepositoryService
        { get; private set; }

        protected virtual void SetupCommand(OleMenuCommand command)
        {}

        public OleMenuCommand Command
        { get; private set; }

        protected bool IsSourceControlProviderActive
        { get; private set; }

        public CommandID CommandID
        { get; private set; }

        protected MenuCommand(Guid menuGroup, int commandId)
        { CommandID = new CommandID(menuGroup, commandId); }

        public void PostExec(object args = null)
        {
            var shell = ServiceProvider.GetService<SVsUIShell, IVsUIShell>();

            var guid = CommandID.Guid;
            ErrorHandler.ThrowOnFailure(
                shell.PostExecCommand(ref guid, (uint)CommandID.ID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref args)
            );
        }

        public void SourceControlProviderDeactivated()
        {
            IsSourceControlProviderActive = false;
            if (Command != null)
                OnSourceControlProviderDeactivated();
        }

        public void SourceControlProviderActivated()
        {
            IsSourceControlProviderActive = true;
            if (Command != null)
            {
                OnSourceControlProviderActivated();
                DoUpdateStatus(this, EventArgs.Empty);
            }
        }

        protected virtual void OnSourceControlProviderDeactivated()
        {
            Command.Enabled = false;
            Command.Visible = false;
        }

        protected virtual void OnSourceControlProviderActivated()
        { Command.Visible = true; }

        protected void UpdateStatus(object sender, EventArgs e)
        {
            DoUpdateStatus(sender, e);

            if (IsSourceControlProviderActive)
                OnSourceControlProviderActivated();
            else
                OnSourceControlProviderDeactivated();
        }

        protected virtual void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = RepositoryService.IsGitRepository; }

        protected virtual void OnStatusChanged(object sender, EventArgs e)
        {}

        protected abstract void Execute(object sender, OleMenuCmdEventArgs e);

        #region Implementation of IPartImportsSatisfiedNotification

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public virtual void OnImportsSatisfied()
        {
            var service = ServiceProvider.GetService<IMenuCommandService, OleMenuCommandService>();
            if (service == null)
                return;

            Command = new OleMenuCommand(
                (sender, e) => Execute(sender, e as OleMenuCmdEventArgs), 
                OnStatusChanged, 
                UpdateStatus, 
                CommandID
            );
            SetupCommand(Command);
            service.AddCommand(Command);
 
            SourceControlProvider.Activated += (sender, e) => SourceControlProviderActivated();
            SourceControlProvider.Deactivated += (sender, e) => SourceControlProviderDeactivated();
       }

        #endregion
    }
}