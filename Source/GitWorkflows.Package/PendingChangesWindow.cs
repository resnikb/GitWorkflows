using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Services;
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
        public string CommitMessage
        { get; set; }

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public PendingChangesWindow() 
            : base(null)
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

            base.Content = new ContentControl();
        }

        public override void  OnToolWindowCreated()
        {
 	        base.OnToolWindowCreated();

            var package = (GitWorkflowsPackage)Package;

            var viewService = package.PartContainer.GetExportedValue<IViewService>();
            var pendingChangesViewModel = package.PartContainer.GetExportedValue<PendingChangesViewModel>();
            
            var contentControl = (ContentControl)base.Content;
            contentControl.Content = viewService.CreateView(pendingChangesViewModel);
        }
    }
}
