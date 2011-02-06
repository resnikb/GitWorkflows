using GitWorkflows.Controls.ViewModels;

namespace GitWorkflows.Package.Interfaces
{
    interface IDialogService
    {
        bool ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel:ViewModel;
    }
}