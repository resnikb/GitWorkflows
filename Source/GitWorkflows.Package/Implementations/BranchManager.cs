using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Package.Common;
using GitWorkflows.Package.Git;
using GitWorkflows.Package.Interfaces;
using NLog;

namespace GitWorkflows.Package.Implementations
{
    [Export(typeof(IBranchManager))]
    class BranchManager : IBranchManager, IPartImportsSatisfiedNotification
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(BranchManager).FullName);

        [Import]
        private IGitService _gitService;

        [Import]
        private ISolutionService _solutionService;

        private readonly Cache<Branch> _currentBranch;
        private readonly Cache<IEnumerable<Branch>> _branches;

        public Branch CurrentBranch
        {
            get { return _currentBranch.Value; }
        }

        public IEnumerable<Branch> Branches
        {
            get { return _branches.Value; }
        }

        public BranchManager()
        {
            _currentBranch = new Cache<Branch>(
                () => _gitService.IsRepositoryOpen 
                            ? new Branch(_gitService.Git.Execute(new Git.Commands.SymbolicRef {Name="HEAD"}))
                            : null
            );
            
            _branches = new Cache<IEnumerable<Branch>>(
                () => _gitService.IsRepositoryOpen 
                            ? _gitService.Git.Execute(new Git.Commands.GetBranches()).Select(name => new Branch(name)).ToArray()
                            : Enumerable.Empty<Branch>()
            );
        }

        public Branch Checkout(string name, bool force)
        {
            var command = new Git.Commands.Checkout {Name = name, Force = force};
            _currentBranch.Invalidate();
            _gitService.Git.Execute(command);
            _solutionService.Reload();
            return CurrentBranch;
        }

        public Branch Create(string name, bool checkout)
        {
            _branches.Invalidate();
            if (checkout)
            {
                var checkoutCommand = new Git.Commands.Checkout { CreateBranch = true, Name = name };
                _currentBranch.Invalidate();
                _gitService.Git.Execute(checkoutCommand);
                _solutionService.Reload();
                return CurrentBranch;
            }

            var branchCommand = new Git.Commands.Branch {Name = name};
            _gitService.Git.Execute(branchCommand);
            return new Branch(name);
        }

        private void InvalidateCache()
        {
            _branches.Invalidate();
            _currentBranch.Invalidate();
        }

        #region Implementation of IPartImportsSatisfiedNotification

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            _solutionService.SolutionClosed += InvalidateCache;
            _solutionService.SolutionOpening += path => InvalidateCache();

            _gitService.RepositoryChanged += (sender, e) => InvalidateCache();
        }

        #endregion
    }
}