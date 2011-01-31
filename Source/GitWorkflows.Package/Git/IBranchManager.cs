using System.Collections.Generic;

namespace GitWorkflows.Package.Git
{
    interface IBranchManager
    {
        Branch CurrentBranch
        { get; }
        
        IEnumerable<Branch> Branches
        { get; }

        Branch Checkout(string name, bool force = false);
        Branch Create(string name, bool checkout = false);        
    }
}