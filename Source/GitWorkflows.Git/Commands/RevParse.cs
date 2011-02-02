using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public sealed class RevParse : Command<string>
    {
        public enum PropertyOption
        {
            TopLevelDirectory,
            Prefix
        }

        public PropertyOption Property
        { get; private set; }

        public RevParse(PropertyOption property)
        { Property = property; }

        public override void Setup(Runner runner)
        {
            runner.Arguments("rev-parse");

            switch (Property)
            {
                case PropertyOption.Prefix:
                    runner.Arguments("--show-prefix");
                    break;

                case PropertyOption.TopLevelDirectory:
                    runner.Arguments("--show-toplevel");
                    break;
            }
        }
    }
}