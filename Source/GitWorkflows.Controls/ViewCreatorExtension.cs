using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Markup;
using GitWorkflows.Services;

namespace GitWorkflows.Controls
{
    [MarkupExtensionReturnType(typeof(Control))]
    public class ViewCreatorExtension : MarkupExtension
    {
        private readonly string _viewModel;

        public static CompositionContainer Container
        { get; set; }

        public ViewCreatorExtension(string viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel))
                throw new ArgumentException("View model type name not specified");

            _viewModel = viewModel;
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension. 
        /// </summary>
        /// <returns>
        /// The object value to set on the property where the extension is applied. 
        /// </returns>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var viewService = Container.GetExportedValue<IViewService>();
            if (viewService == null)
                throw new InvalidOperationException("Cannot retrieve view service.");

            var exports = Container.GetExports(new ImportDefinition(e => e.ContractName.EndsWith("."+_viewModel), null, ImportCardinality.ExactlyOne, true, true));
            var viewModel = exports.Single().Value;
            if (viewModel == null)
                throw new InvalidOperationException("Cannot create viewmodel: "+_viewModel);

            return viewService.CreateView(viewModel);
        }
    }
}