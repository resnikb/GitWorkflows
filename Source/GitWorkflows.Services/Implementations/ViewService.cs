using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GitWorkflows.Services.Implementations
{
    [Export(typeof(IViewService))]
    public class ViewService : IViewService
    {
        [Import]
        private CompositionContainer _container;

        public void ShowDialog<TViewModel>(
            TViewModel viewModel, 
            Action<TViewModel> onSuccess, 
            Action<TViewModel> onCancel, 
            Action<TViewModel, bool> afterClose
        )
        {
            Control contentControl;
            var export = CreateView(viewModel, out contentControl);

            try
            {
                var window = CreateWindow();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.SizeToContent = SizeToContent.WidthAndHeight;
                window.ResizeMode = ResizeMode.NoResize;
                window.Content = contentControl;
                window.Title = export.Metadata.WindowTitle;

                var dialogResult = ShowDialog(window);
                if (dialogResult == true)
                {
                    if (onSuccess != null)
                        onSuccess(viewModel);
                }
                else if (onCancel != null)
                    onCancel(viewModel);

                if (afterClose != null)
                    afterClose(viewModel, dialogResult == true);
            }
            finally
            {
                _container.ReleaseExport(export);
            }
        }

        public Control CreateView<TViewModel>(TViewModel viewModel)
        {
            Control contentControl;
            CreateView(viewModel, out contentControl);
            return contentControl;
        }

        protected virtual Window CreateWindow()
        { return new Window(); }

        protected virtual bool? ShowDialog(Window window)
        { return window.ShowDialog(); }

        protected Lazy<Control, IViewMetadata> CreateView(object viewModel, out Control contentControl)
        {
            var exportAttribute = viewModel.GetType().GetCustomAttributes(false).OfType<ExportAttribute>().SingleOrDefault();
            
            string contractName;
            if (exportAttribute == null)
                contractName = viewModel.GetType().Name;
            else if (!string.IsNullOrEmpty(exportAttribute.ContractName))
                contractName = exportAttribute.ContractName;
            else
                contractName = exportAttribute.ContractType.Name;

            Lazy<Control, IViewMetadata> export;
            try
            {
                export = _container.GetExport<Control, IViewMetadata>(contractName);
            }
            catch (ImportCardinalityMismatchException e)
            {
                throw new ViewNotFoundException(contractName, e);
            }

            if (export == null)
                throw new ViewNotFoundException(contractName);

            contentControl = export.Value;
            contentControl.DataContext = viewModel;
            return export;
        }
    }
}