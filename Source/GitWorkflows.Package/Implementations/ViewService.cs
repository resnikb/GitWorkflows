using System.ComponentModel.Composition;
using System.Windows;
using GitWorkflows.Services;
using Microsoft.VisualStudio.PlatformUI;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(IViewService))]
    class ViewService : Services.Implementations.ViewService
    {
        protected override Window CreateWindow()
        { return new DialogWindow(); }

        protected override bool? ShowDialog(Window window)
        { return ((DialogWindow)window).ShowModal(); }
    }
}