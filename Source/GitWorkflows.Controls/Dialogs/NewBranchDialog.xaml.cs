﻿using System.ComponentModel.Composition;
using System.Windows;

namespace GitWorkflows.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for NewBranchDialog.xaml
    /// </summary>
    [Export("NewBranchViewModel", typeof(Window))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class NewBranchDialog
    {
        public NewBranchDialog()
        { InitializeComponent(); }
    }
}