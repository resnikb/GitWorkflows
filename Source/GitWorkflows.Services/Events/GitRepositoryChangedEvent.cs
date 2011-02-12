using System.Collections.Generic;
using System.ComponentModel.Composition;
using GitWorkflows.Common;
using Microsoft.Practices.Prism.Events;

namespace GitWorkflows.Services.Events
{
    /// <summary>
    /// Event that is published when the contents of the git repository are modified.
    /// </summary>
    /// 
    /// <remarks>
    ///     <para>When changes are detected inside .git directory, this event is published. The
    ///     payload of the event is the set of all paths that were modified.</para>
    /// </remarks>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class GitRepositoryChangedEvent : CompositePresentationEvent<HashSet<Path>>
    {}
}