using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Services.Events
{
    /// <summary>
    /// Event that is published when a branch is created or deleted.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class GitBranchCollectionChangedEvent : CompositePresentationEvent<IBranchManager>
    {}
}