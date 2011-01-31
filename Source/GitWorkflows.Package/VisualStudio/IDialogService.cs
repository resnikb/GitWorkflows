using GitWorkflows.Package.ViewModels;

namespace GitWorkflows.Package.VisualStudio
{
    interface IDialogService
    {
        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel:ViewModel;
    }
}