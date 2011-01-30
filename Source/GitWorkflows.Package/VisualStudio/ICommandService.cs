using System.Collections.Generic;
using System.ComponentModel.Design;

namespace GitWorkflows.Package.VisualStudio
{
    interface ICommandService
    {
        IEnumerable<MenuCommand> Commands
        { get; }
        
        bool ExecuteLater(MenuCommand command, object args);    
        bool ExecuteLater(CommandID command, object args);    
    }
}