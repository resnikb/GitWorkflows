using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Microsoft.VisualStudio.PlatformUI;

namespace GitWorkflows.Package.VisualStudio
{
    [Export(typeof(IDialogService))]
    class DialogService : IDialogService
    {
        [Import]
        private ExportProvider _exportProvider;

        public bool? ShowDialog<TViewModel>(TViewModel viewModel)
        {
            var window = _exportProvider.GetExportedValue<DialogWindow>(typeof(TViewModel).Name);
            window.DataContext = viewModel;
            return window.ShowModal();
        }
    }
}