using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Git;
using GitWorkflows.Services;
using GitWorkflows.Services.Events;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Controls.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class PendingChangesViewModel : ViewModel
    {
        private readonly IRepositoryService _repositoryService;
        private readonly IFileIconService _iconService;
        private readonly List<StatusViewModel> _selectedItems = new List<StatusViewModel>();

        public ObservableCollection<StatusViewModel> Changes
        { get; private set; }

        [ImportingConstructor]
        public PendingChangesViewModel(
            IRepositoryService repositoryService, 
            IFileIconService iconService,
            GitWorkingTreeChangedEvent workingTreeChangedEvent
        )
        {
            _repositoryService = repositoryService;
            _iconService = iconService;
            Changes = new ObservableCollection<StatusViewModel>();
                
            workingTreeChangedEvent.Subscribe(Refresh, ThreadOption.UIThread);
            Refresh(repositoryService);
        }

        private void Refresh(IRepositoryService repositoryService)
        {
            Changes.Clear();
            repositoryService.Status.Statuses
                .Where(s => (s.FileStatus & FileStatus.Ignored) == 0)
                .Select(s => new StatusViewModel(repositoryService, _iconService, s))
                .ForEach(Changes.Add);
        }

        [CommandExecute("SelectionChanged")]
        public void SelectionChanged(IList selectedItems)
        {
            _selectedItems.Clear();
            if (selectedItems != null)
            {
                _selectedItems.Capacity = Math.Max(_selectedItems.Capacity, selectedItems.Count);
                _selectedItems.AddRange(selectedItems.Cast<StatusViewModel>());
            }

            GetProperty<DelegateCommandBase>("CommandViewDifferences").RaiseCanExecuteChanged();
            GetProperty<DelegateCommandBase>("CommandResetChanges").RaiseCanExecuteChanged();
        }

        [CommandExecute("CommandViewDifferences")]
        public void ViewDifferencesForSelectedItem()
        {
            _repositoryService.DisplayUnstagedChangesAsync(_selectedItems.Single().Status.FilePath);
        }

        [CommandCanExecute("CommandViewDifferences")]
        public bool CanViewDifferencesForSelection()
        {
            return _selectedItems.Count == 1 && (_selectedItems[0].Status.FileStatus & FileStatus.Modified) != 0;
        }

        [CommandExecute("CommandResetChanges")]
        public void ResetChangesForSelectedItems()
        {
            _repositoryService.ResetChanges(_selectedItems.Select(vm => vm.Status.FilePath));
        }

        [CommandCanExecute("CommandResetChanges")]
        public bool IsAnythingSelected()
        {
            return _selectedItems.Count > 0;
        }
    }
}