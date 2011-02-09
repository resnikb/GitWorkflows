using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Git.Commands;
using NLog;

namespace GitWorkflows.Git
{
    [Export(typeof(IBranchManager))]
    class BranchManager : NotifyPropertyChanged, IBranchManager, IPartImportsSatisfiedNotification
    {
        private static readonly Logger Log = LogManager.GetLogger(typeof(BranchManager).FullName);

        [Import]
        private IRepositoryService _repositoryService;

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
                var checkoutCommand = new Checkout { CreateBranch = true, Name = name };
                _repositoryService.Git.Execute(checkoutCommand);
            }
            else
            {
                var branchCommand = new Commands.Branch {Name = name};
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
            
            var command = new Checkout {Name = name};
            _repositoryService.Git.Execute(command);
        }

        #region Implementation of IPartImportsSatisfiedNotification

        /// <summary>
        /// Called when a part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            _repositoryService.RepositoryChanged += changedFiles =>
            {
                if (changedFiles == null || changedFiles.Any(_repositoryService.RepositoryDirectory.Combine("refs", "heads").IsParentOf))
                {
                    _branches.Invalidate();
                    RaisePropertyChanged(() => Branches);
                }

                if (changedFiles == null || changedFiles.Contains(_repositoryService.RepositoryDirectory.Combine("HEAD")))
                {
                    _currentBranch.Invalidate();
                    RaisePropertyChanged(() => CurrentBranch);
                }
            };
        }

        #endregion
    }
}