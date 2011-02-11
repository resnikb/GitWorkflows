using System.Collections.Generic;
using GitWorkflows.Git;

namespace GitWorkflows.Services
{
    public interface IBranchManager
    {
        IEnumerable<Branch> Branches
        { get; }

        Branch CurrentBranch
        { get; }

        Branch Create(string name, bool checkout);
        void Delete(string name);
        void Checkout(string name);
    }
}