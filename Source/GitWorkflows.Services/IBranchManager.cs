using System.Collections.Generic;
using System.ComponentModel;
using GitWorkflows.Git;

namespace GitWorkflows.Services
{
    public interface IBranchManager : INotifyPropertyChanged
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