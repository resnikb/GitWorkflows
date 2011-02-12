using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Services.Events
{
    /// <summary>
    /// Event that is published when the current branch is changed. It is fired whenever a revision
    /// is checked out.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class GitCurrentBranchChangedEvent : CompositePresentationEvent<IBranchManager>
    {}
}