using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;
using GitWorkflows.Package.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using GitWorkflows.Package.Extensions;
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

        public bool ExecuteLater(MenuCommand command, object args)
        { return ExecuteLater(command.CommandID, args); }

        public bool ExecuteLater(CommandID command, object args)
        {
            var shell = _serviceProvider.GetService<SVsUIShell, IVsUIShell>();

            var guid = command.Guid;
            var a = args;

            ErrorHandler.ThrowOnFailure( 
                shell.PostExecCommand(ref guid, (uint)command.ID, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, ref a)
            );
            return true;
        }

        public bool Execute<TCommand>(object args) where TCommand : MenuCommand
        {
            var command = Commands.OfType<TCommand>().Single();
            command.Command.Invoke(args);
            return true;
        }
    }
}