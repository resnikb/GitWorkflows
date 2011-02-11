using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Services.Events
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class GitWorkingTreeChangedEvent : CompositePresentationEvent<IRepositoryService>
    {}
}