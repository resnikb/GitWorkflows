using System.Collections.Generic;
using System.ComponentModel.Design;
using MenuCommand = GitWorkflows.Package.VisualStudio.MenuCommand;

namespace GitWorkflows.Package.Interfaces
{
    interface ICommandService
    {
        IEnumerable<MenuCommand> Commands
        { get; }
        
        void ExecuteLater<TCommand>(object args = null) where TCommand:MenuCommand;    
        void ExecuteLater(MenuCommand command, object args = null);    
        void ExecuteLater(CommandID command, object args = null);    
        void Execute<TCommand>(object args = null) where TCommand:MenuCommand;
    }
}