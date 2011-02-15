using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GitWorkflows.Common;
using GitWorkflows.Git;
using GitWorkflows.Services;

namespace GitWorkflows.Controls.ViewModels
{
    public class StatusViewModel : ViewModel
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

        public string StatusText
        { get; private set; }

        public string FullPath
        { get; private set; }

        public Status Status
        { get; private set; }

        public StatusViewModel(IRepositoryService repositoryService, Status status)
        {
            Status = status;
            PathInRepository = status.FilePath.GetRelativeTo(repositoryService.BaseDirectory);
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