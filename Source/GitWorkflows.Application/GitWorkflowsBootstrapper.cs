using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Windows;
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
        { return new MainWindow(); }

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
            var thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            AggregateCatalog.Catalogs.Add(new DirectoryCatalog(System.IO.Path.GetDirectoryName(thisAssemblyPath)));
        }
    }
}