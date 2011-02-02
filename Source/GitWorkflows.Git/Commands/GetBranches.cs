using System.Linq;
using GitWorkflows.Common;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public class GetBranches : Command<string[]>
    {
        public override void Setup(Runner runner)
        {
            runner.Arguments("branch");
        }

        protected override string[] Parse(ApplicationDefinition app, string contents)
        {
            return contents.GetLines()
                           .Select(line => line[0] == '*' ? line.Substring(1).Trim() : line.Trim())
                           .ToArray();
        }
    }
}