using System;
using System.Collections.Generic;
using GitWorkflows.Package.Git;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitWorkflows.Package.VisualStudio
{
    internal interface ISolutionService
    {
        event EventHandler SolutionChanged;
        event EventHandler WorkingTreeChanged;
        event EventHandler RepositoryChanged;

        bool IsControlledByGit
        { get; }

        WorkingTree WorkingTree
        { get; }

        void Reload();
        void SaveAllDocuments();
        void RefreshSourceControlGlyphs(IEnumerable<VSITEMSELECTION> nodes = null);
    }
}