using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Services.Events
{
    /// <summary>
    /// Event that is published when the status of the working tree has changed. This can happen
    /// when any tracked file is modified, when a branch is checked out or when files are reset.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class GitWorkingTreeChangedEvent : CompositePresentationEvent<IRepositoryService>
    {}
}