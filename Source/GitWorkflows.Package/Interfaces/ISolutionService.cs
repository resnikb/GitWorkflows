using GitWorkflows.Common;

namespace GitWorkflows.Package.Interfaces
{
    interface ISolutionService
    {
        void RefreshSourceControlIcons();
        void Reload();
        string GetNameOfParentProject(Path path);
    }
}