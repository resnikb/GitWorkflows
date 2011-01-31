using System.Collections.Generic;
using System.ComponentModel.Design;
using MenuCommand = GitWorkflows.Package.VisualStudio.MenuCommand;

namespace GitWorkflows.Package.Interfaces
{
    interface ICommandService
    {
        IEnumerable<MenuCommand> Commands
        { get; }
        
        bool ExecuteLater(MenuCommand command, object args);    
        bool ExecuteLater(CommandID command, object args);    
        bool Execute<TCommand>(object args) where TCommand:MenuCommand;
    }
}