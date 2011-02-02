using System;
using GitWorkflows.Common.Subprocess;

namespace GitWorkflows.Git.Commands
{
    public abstract class Command<T>
    {
        public virtual T ProcessResult(ApplicationDefinition app, Tuple<int, string> result)
        {
            if (result.Item1 != 0)
                ThrowException(app, result);

            return Parse(app, result.Item2);
        }

        protected virtual void ThrowException(ApplicationDefinition app, Tuple<int, string> result)
        { throw new Exception(result.Item2); }

        protected virtual T Parse(ApplicationDefinition app, string contents)
        { return (T)(object)contents; }

        public abstract void Setup(Runner runner);
    }
}