using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Package.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = System.IServiceProvider;
using MenuCommand = GitWorkflows.Package.VisualStudio.MenuCommand;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(ICommandService))]
    class CommandService : ICommandService
    {
        [Import]
        private IServiceProvider _serviceProvider;

        [ImportMany]
        public IEnumerable<MenuCommand> Commands
        { get; set; }

        public void ExecuteLater<T>(object args) where T:MenuCommand
        { ExecuteLater(Commands.OfType<T>().Single(), args); }

        public void ExecuteLater(MenuCommand command, object args)
        { ExecuteLater(command.CommandID, args); }

        public void ExecuteLater(CommandID command, object args)
        {
            var shell = _serviceProvider.GetService<SVsUIShell, IVsUIShell>();

            var guid = command.Guid;
            ErrorHandler.ThrowOnFailure( 
                shell.PostExecCommand(ref guid, (uint)command.ID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref args)
            );
        }

        public void Execute<TCommand>(object args) where TCommand : MenuCommand
        {
            var command = Commands.OfType<TCommand>().Single();
            command.Command.Invoke(args);
        }
    }
}