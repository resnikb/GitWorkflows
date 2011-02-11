using GitWorkflows.Common;

namespace GitWorkflows.Controls.ViewModels
{
    public abstract class ViewModel : NotifyPropertyChanged
    {
#pragma warning disable 1911
        protected override void RaisePropertyChanged(string propertyName)
        { UIDispatcher.Schedule(() => base.RaisePropertyChanged(propertyName)); }
#pragma warning restore 1911
    }
}