namespace GitWorkflows.Package.VisualStudio
{
    interface IDialogService
    {
        bool? ShowDialog<TViewModel>(TViewModel viewModel);
    }
}