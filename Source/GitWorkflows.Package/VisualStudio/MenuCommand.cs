using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using GitWorkflows.Package.Extensions;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitWorkflows.Package.VisualStudio
{
    abstract class MenuCommand
    {
        private IServiceProvider _serviceProvider;

        [Import]
        protected IServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
            private set
            {
                _serviceProvider = value;
                
                var service = _serviceProvider.GetService<IMenuCommandService, OleMenuCommandService>();
                if (service == null)
                    return;

                Command = new OleMenuCommand(Execute, OnStatusChanged, UpdateStatus, CommandID);
                SetupCommand(Command);
                service.AddCommand(Command);
            }
        }

        [Import]
        protected SourceControlProvider SourceControlProvider
        {
            set
            {
                value.Activated += (sender, e) => SourceControlProviderActivated();
                value.Deactivated += (sender, e) => SourceControlProviderDeactivated();
            }
        }

        [Import]
        protected Microsoft.VisualStudio.Shell.Package HostPackage
        { get; set; }

        protected virtual void SetupCommand(OleMenuCommand command)
        {}

        protected OleMenuCommand Command
        { get; private set; }

        protected bool IsSourceControlProviderActive
        { get; private set; }

        public CommandID CommandID
        { get; private set; }

        protected MenuCommand(Guid menuGroup, int commandId)
        { CommandID = new CommandID(menuGroup, commandId); }

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

        public void Execute()
        {
            if (Command != null)
            {
                DoUpdateStatus(this, EventArgs.Empty);
                if (Command.Enabled && Command.Visible)
                    Execute(this, EventArgs.Empty);
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
        {}

        protected virtual void OnStatusChanged(object sender, EventArgs e)
        {}

        protected abstract void Execute(object sender, EventArgs e);
    }
}