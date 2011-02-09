using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Common;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Git;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("459f8ad1-6802-4d74-bd43-86fe92ed898b")]
    public class PendingChangesWindow : ToolWindowPane
    {
        public ObservableCollection<Status> Status
        { get; private set; }

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public PendingChangesWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            Caption = Resources.ToolWindowTitle;

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;

            Status = new ObservableCollection<Status>();

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = new PendingChangesControl { DataContext = this };
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
            
            var package = (GitWorkflowsPackage)Package;
            var repositoryService = package.PartContainer.GetExportedValue<IRepositoryService>();
            repositoryService.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "Status")
                    ViewModel.ExecuteOnDispatcher(() => Refresh(repositoryService));
            };

            // Refresh now, to initialize with any changes
            Refresh(repositoryService);
        }

        private void Refresh(IRepositoryService repositoryService)
        {
            Status.Clear();
            repositoryService.Status.Statuses
                .Where(s => (s.FileStatus & FileStatus.Ignored) == 0)
                .ForEach(Status.Add);
        }
    }
}
