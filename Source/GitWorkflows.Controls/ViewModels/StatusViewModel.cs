using System.Windows.Media;
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

        public StatusViewModel(IRepositoryService repositoryService, IFileIconService iconService, Status status)
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

            Icon = iconService.GetIcon(status.FilePath);
        }
    }
}