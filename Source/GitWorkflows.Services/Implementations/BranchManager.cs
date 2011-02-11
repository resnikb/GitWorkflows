using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Git.Commands;
using GitWorkflows.Services.Events;
using NLog;
using Branch = GitWorkflows.Git.Branch;

namespace GitWorkflows.Services.Implementations
{
    [Export(typeof(IBranchManager))]
    class BranchManager : IBranchManager, IPartImportsSatisfiedNotification
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(BranchManager).FullName);

        [Import]
        private IRepositoryService _repositoryService;

        [Import]
        private GitRepositoryChangedEvent _repositoryChangedEvent;

        [Import]
        private GitBranchCollectionChangedEvent _branchCollectionChangedEvent;

        [Import]
        private GitCurrentBranchChangedEvent _currentBranchChangedEvent;

        private readonly CachedValue<Branch> _currentBranch;
        private readonly CachedValue<IEnumerable<Branch>> _branches;

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
            _currentBranch = new CachedValue<Branch>(
                () => _repositoryService.IsGitRepository
                            ? new Branch(_repositoryService.Git.Execute(new SymbolicRef {Name="HEAD"}))
                            : null
            );
            
            _branches = new CachedValue<IEnumerable<Branch>>(
                () => _repositoryService.IsGitRepository 
                            ? _repositoryService.Git.Execute(new GetBranches()).Select(name => new Branch(name)).ToArray()
                            : Enumerable.Empty<Branch>()
            );
        }

        public Branch Create(string name, bool checkout)
        {
            if (checkout)
            {
                var checkoutCommand = new Checkout { CreateBranch = true, BranchName = name };
                _repositoryService.Git.Execute(checkoutCommand);
            }
            else
            {
                var branchCommand = new Git.Commands.Branch {Name = name};
                _repositoryService.Git.Execute(branchCommand);
            }

            return new Branch(name);
        }

        public void Delete(string name)
        {
            throw new NotImplementedException();
        }

        public void Checkout(string name)
        {
            _currentBranch.Invalidate();
            
            var command = new Checkout {BranchName = name};
            _repositoryService.Git.Execute(command);
        }

        #region Implementation of IPartImportsSatisfiedNotification

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            _repositoryChangedEvent.Subscribe( 
                changedFiles =>
                {
                    if (changedFiles == null || changedFiles.Any(_repositoryService.RepositoryDirectory.Combine("refs", "heads").IsParentOf))
                    {
                        _branches.Invalidate();
                        _branchCollectionChangedEvent.Publish(this);
                    }

                    if (changedFiles == null || changedFiles.Contains(_repositoryService.RepositoryDirectory.Combine("HEAD")))
                    {
                        _currentBranch.Invalidate();
                        _currentBranchChangedEvent.Publish(this);
                    }
                }
            );
        }

        #endregion
    }
}