using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.InteropServices;
using GitWorkflows.Common;
using GitWorkflows.Package.Interfaces;
using GitWorkflows.Package.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace GitWorkflows.Package.PackageCommands
{
    [Export(typeof(MenuCommand))]
    class CommandBranchComboBox : MenuCommand
    {
        [Import]
        private IBranchManager _branchManager;

        [Import]
        private ICommandService _commandService;

        public CommandBranchComboBox() 
            : base(Constants.guidPackageCmdSet, Constants.idBranchCombo)
        {}

        protected override void SetupCommand(OleMenuCommand command)
        {
            // accept any argument string
            command.ParametersDescription = "$";
        }

        // DynamicCombo
        //   A DYNAMICCOMBO allows the user to type into the edit box or pick from the list. The 
        //	 list of choices is usually fixed and is managed by the command handler for the command.
        //	 For example, this type of combo is used for the "Zoom" combo on the "Class Designer" toolbar.
        //
        //   A Combo box requires two commands:
        //     One command is used to ask for the current value of the combo box and to set the new value when the user
        //     makes a choice in the combo box.
        //
        //     The second command is used to retrieve this list of choices for the combo box.
        protected override void Execute(object sender, OleMenuCmdEventArgs e)
        {
            var input = e.InValue;
            var vOut = e.OutValue;

            if (vOut != IntPtr.Zero)
            {
                // when vOut is non-NULL, the IDE is requesting the current value for the combo
                Marshal.GetNativeVariantForObject(_branchManager.CurrentBranch.Name, vOut);
            }
            else if (input != null)
            {
                // new branch name was selected or typed in
                var newBranch = input.ToString();
                if (newBranch != _branchManager.CurrentBranch.Name)
                {
                    if (_branchManager.Branches.FirstOrDefault(b => b.Name == newBranch) != null) 
                        _branchManager.Checkout(newBranch);
                    else
                        _commandService.Execute<CommandNewBranch>(new EventArgs<string>(newBranch));
                }
            }
        }

        protected override void DoUpdateStatus(object sender, EventArgs e)
        { Command.Enabled = _branchManager.Branches.Any(); }
    }
}