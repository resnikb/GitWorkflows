using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GitWorkflows.Common;
using GitWorkflows.Controls;
using GitWorkflows.Git;
using GitWorkflows.Services;
using GitWorkflows.Services.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.VisualStudio.Shell;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Status = GitWorkflows.Git.Status;

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
        private IRepositoryService _repositoryService;

        public ObservableCollection<PendingChangeViewModel> Changes
        { get; private set; }

        private readonly List<PendingChangeViewModel> _selectedItems = new List<PendingChangeViewModel>();

        public string CommitMessage
        { get; set; }

        public DelegateCommand<object> CommandViewDifferences
        { get; private set; }

        public DelegateCommand<object> CommandResetChanges
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

            Changes = new ObservableCollection<PendingChangeViewModel>();

            CommandViewDifferences = new DelegateCommand<object>(
                _ => _repositoryService.DisplayUnstagedChangesAsync(_selectedItems.Single().Status.FilePath),
                _ => _selectedItems.Count == 1 && (_selectedItems[0].Status.FileStatus & FileStatus.Modified) != 0
            );

            CommandResetChanges = new DelegateCommand<object>(
                _ => _repositoryService.ResetChanges(_selectedItems.Select(vm => vm.Status.FilePath)),
                _ => _selectedItems.Count > 0
            );

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = new PendingChangesControl { DataContext = this };
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();
            
            var package = (GitWorkflowsPackage)Package;

            var workingTreeChangedEvent = package.PartContainer.GetExportedValue<GitWorkingTreeChangedEvent>();
            workingTreeChangedEvent.Subscribe(Refresh, ThreadOption.UIThread);

            // Refresh now, to initialize with any changes
            _repositoryService = package.PartContainer.GetExportedValue<IRepositoryService>();
            Refresh(_repositoryService);
        }

        private void Refresh(IRepositoryService repositoryService)
        {
            Changes.Clear();
            repositoryService.Status.Statuses
                .Where(s => (s.FileStatus & FileStatus.Ignored) == 0)
                .Select(s => new PendingChangeViewModel(repositoryService, s))
                .ForEach(Changes.Add);
        }

        public void SelectionChanged(IList selectedItems)
        {
            _selectedItems.Clear();
            if (selectedItems != null)
            {
                _selectedItems.Capacity = Math.Max(_selectedItems.Capacity, selectedItems.Count);
                _selectedItems.AddRange(selectedItems.Cast<PendingChangeViewModel>());
            }

            CommandViewDifferences.RaiseCanExecuteChanged();
            CommandResetChanges.RaiseCanExecuteChanged();
        }
    }

    public class PendingChangeViewModel
    {
        private static readonly Brush _brushModified  = Brushes.Blue;
        private static readonly Brush _brushStaged    = Brushes.Purple;
        private static readonly Brush _brushUntracked = Brushes.Black;
        private static readonly Brush _brushDeleted   = Brushes.Red;
        private static readonly Brush _brushDefault   = Brushes.Gray;

        public Brush StatusColor
        { get; private set; }

        public bool IsSelected
        { get; set; }

        public ImageSource Icon
        { get; private set; }

        public string PathInRepository
        { get; private set; }

        public string ProjectName
        { get; private set; }

        public string StatusText
        { get; private set; }

        public string FullPath
        { get; private set; }

        public Status Status
        { get; private set; }

        public PendingChangeViewModel(IRepositoryService repositoryService, Status status)
        {
            Status = status;
            PathInRepository = status.FilePath.GetRelativeTo(repositoryService.BaseDirectory);
            ProjectName = string.Empty;
            StatusText = status.FileStatus.ToString();
            FullPath = status.FilePath;

            if ( (status.FileStatus & FileStatus.Modified) != 0 )
                StatusColor = _brushModified;
            else if (status.FileStatus == FileStatus.Untracked)
                StatusColor = _brushUntracked;
            else if (status.FileStatus == FileStatus.Added)
                StatusColor = _brushStaged;
            else if (status.FileStatus == FileStatus.Removed || status.FileStatus == FileStatus.RenameSource)
                StatusColor = _brushDeleted;
            else
                StatusColor = _brushDefault;

            Icon = CreateIcon(status.FilePath);
        }

        private static ImageSource CreateIcon(string path)
        {
            var shinfo = new PInvoke.SHFILEINFO();
            PInvoke.SHGetFileInfo(path, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), PInvoke.SHGFI_ICON | PInvoke.SHGFI_SMALLICON);

            var iconHandle = shinfo.hIcon;
            if (IntPtr.Zero == iconHandle)
                return null;

            try
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    iconHandle,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );
            }
            finally
            {
                PInvoke.DestroyIcon(iconHandle);
            }
        }
    }
}
