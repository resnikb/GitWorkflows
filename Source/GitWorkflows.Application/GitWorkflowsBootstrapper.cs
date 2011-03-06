using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows;
using GitWorkflows.Common;
using GitWorkflows.Services;
using Microsoft.Practices.Prism.MefExtensions;

namespace GitWorkflows.Application
{
    class GitWorkflowsBootstrapper : MefBootstrapper
    {
        /// <summary>
        /// Creates the shell or main window of the application.
        /// </summary>
        /// 
        /// <returns>
        /// The shell of the application.
        /// </returns>
        /// 
        /// <remarks>
        /// If the returned instance is a <see cref="T:System.Windows.DependencyObject"/>, the
        /// <see cref="T:Microsoft.Practices.Prism.Bootstrapper"/> will attach the default 
        /// <seealso cref="T:Microsoft.Practices.Prism.Regions.IRegionManager"/>of the application
        /// in its 
        /// <see cref="F:Microsoft.Practices.Prism.Regions.RegionManager.RegionManagerProperty"/>
        /// attached property in order to be able to add regions by using the 
        /// <seealso cref="F:Microsoft.Practices.Prism.Regions.RegionManager.RegionNameProperty"/>
        /// attached property from XAML.
        /// </remarks>
        protected override DependencyObject CreateShell()
        {
            var viewModel = Container.GetExportedValue<ShellViewModel>();
            var viewService = Container.GetExportedValue<IViewService>();
            return viewService.CreateView(viewModel);
        }

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        /// <remarks>
        /// The base implementation ensures the shell is composed in the container.
        /// </remarks>
        protected override void InitializeShell()
        {
            System.Windows.Application.Current.MainWindow = (Window)Shell;
            System.Windows.Application.Current.MainWindow.Show();
        }

        /// <summary>
        /// Configures the 
        /// <see cref="P:Microsoft.Practices.Prism.MefExtensions.MefBootstrapper.AggregateCatalog"/>
        /// used by MEF.
        /// </summary>
        /// <remarks>
        /// The base implementation does nothing.
        /// </remarks>
        protected override void ConfigureAggregateCatalog()
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var appDirectory = System.IO.Path.GetDirectoryName(thisAssembly.Location);

            var catalogs = new[]
            {
                new AssemblyCatalog(thisAssembly),
                new AssemblyCatalog(System.IO.Path.Combine(appDirectory, "GitWorkflows.Services.dll")),
                new AssemblyCatalog(System.IO.Path.Combine(appDirectory, "GitWorkflows.Controls.dll")),
            };

            catalogs.ForEach(AggregateCatalog.Catalogs.Add);
        }

        /// <summary>
        /// Configures the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer"/>.
        /// May be overwritten in a derived class to add specific type mappings required by the application.
        /// </summary>
        /// <remarks>
        /// The base implementation registers all the types direct instantiated by the bootstrapper with the container.
        /// If the method is overwritten, the new implementation should call the base class version.
        /// </remarks>
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

            var compositionBatch = new CompositionBatch();
            compositionBatch.AddExportedValue(Container);

            Container.Compose(compositionBatch);
        }
    }
}