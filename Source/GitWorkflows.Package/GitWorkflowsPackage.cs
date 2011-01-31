using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using GitWorkflows.Package.Interfaces;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using NLog;
using NLog.Config;

namespace GitWorkflows.Package
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio is to
    /// implement the IVsPackage interface and register itself with the shell. This package uses the
    /// helper classes defined inside the Managed Package Framework (MPF) to do it: it derives from
    /// the Package class that provides the implementation of the  IVsPackage interface and uses the
    /// registration attributes defined in the framework to  register itself and its components with
    /// the shell.
    /// </summary>
    
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // Let the shell know we will register the SCC as a service available to everyone
    [ProvideService(typeof(SourceControlProvider), ServiceName = "GitWorkflows Source Control Provider")]
    
    // Pre-load the package when the command UI context is asserted (the provider will be automatically loaded after restarting the shell if it was active last time the shell was shutdown)
    [ProvideAutoLoad(Constants.guidSccProviderString)]

    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(PendingChangesWindow))]

    // Register as source control provider
    [ProvideSourceControlProvider("Git Workflows", "#110")]

    // Declare the package guid
    [Guid(Constants.guidPackagePkgString)]
    public sealed class GitWorkflowsPackage : Microsoft.VisualStudio.Shell.Package
    {
        private AssemblyCatalog _partCatalog;
        private CompositionContainer _partContainer;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public GitWorkflowsPackage()
        {
            Logging.VsOutputPaneTarget.ServiceProvider = this;
            ConfigurationItemFactory.Default.Targets.RegisterDefinition("VsOutputPaneTarget", typeof(Logging.VsOutputPaneTarget));
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            var myDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var nlogConfigPath = System.IO.Path.Combine(myDirectory, "config.nlog");
            LogManager.Configuration = new XmlLoggingConfiguration(nlogConfigPath);

            var logger = LogManager.GetLogger(GetType().FullName);
            logger.Debug("Configuring internal container");

            _partCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            _partContainer = new CompositionContainer(_partCatalog);
            
            var compositionBatch = new CompositionBatch();
            compositionBatch.AddExportedValue<IServiceProvider>(this);
            compositionBatch.AddExportedValue<IServiceContainer>(this);
            compositionBatch.AddExportedValue<Microsoft.VisualStudio.Shell.Package>(this);
            compositionBatch.AddExportedValue<ExportProvider>(_partContainer);

            _partContainer.Compose(compositionBatch);

//            LogManager.OutputWindowService = _partContainer.GetExportedValue<IOutputWindowService>();

            // Create all exported parts
            logger.Debug("Creating parts");
            _partContainer.GetExportedValue<ICommandService>();

            // Add our command handlers for menu (commands must exist in the .vsct file)
//            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
//            if ( null != mcs )
//            {
                // Create the command for the menu item.
//                CommandID menuCommandID = new CommandID(Constants.guidPackageCmdSet, (int)PkgCmdIDList.cmdidCommit);
//                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID );
//                mcs.AddCommand( menuItem );
                // Create the command for the tool window
//                CommandID toolwndCommandID = new CommandID(Constants.guidPackageCmdSet, (int)PkgCmdIDList.cmdidPendingChanges);
//                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
//                mcs.AddCommand( menuToolWin );
//            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _partContainer.Dispose();
                _partCatalog.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "GitWorkflows",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }
    }
}
