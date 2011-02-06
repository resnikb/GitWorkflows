using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Windows;
using System.Windows.Media;
using GitWorkflows.Common;
using GitWorkflows.Controls.ViewModels;
using GitWorkflows.Package.Interfaces;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(IDialogService))]
    class DialogService : IDialogService
    {
        [Import]
        private ExportProvider _exportProvider;

        [Import]
        private IServiceProvider _serviceProvider;

        public bool ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ViewModel
        {
            var window = _exportProvider.GetExportedValue<Window>(typeof(TViewModel).Name);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.DataContext = viewModel;

            var dialogWindow = window as DialogWindow;
            if (dialogWindow != null)
                return dialogWindow.ShowModal() == true;

            window.Background = (Brush) Application.Current.Resources[VsBrushes.EnvironmentBackgroundGradientKey];
            var shell = _serviceProvider.GetService<SVsUIShell, IVsUIShell>();

            IntPtr hwnd;
            shell.GetDialogOwnerHwnd(out hwnd);

            shell.EnableModeless(0);
            try
            {
                return WindowHelper.ShowModal(window, hwnd) != DialogResult.Cancel;
            }
            finally
            {
                shell.EnableModeless(1);
            }
        }
    }
}