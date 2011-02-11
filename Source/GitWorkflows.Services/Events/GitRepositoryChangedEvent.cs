using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitWorkflows.Common;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Services.Events
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class GitRepositoryChangedEvent : CompositePresentationEvent<HashSet<Path>>
    {}
}