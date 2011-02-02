using System;
using GitWorkflows.Common;

namespace GitWorkflows.Package.Interfaces
{
    interface ISolutionService
    {
        event Action SolutionClosed;
        event Action<Path> SolutionOpening;

        void Initialize();
        void Reload();
        void RefreshSourceControlIcons();
    }
}