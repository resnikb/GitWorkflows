using System.Collections.Generic;
using GitWorkflows.Git;

namespace GitWorkflows.Package.Interfaces
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