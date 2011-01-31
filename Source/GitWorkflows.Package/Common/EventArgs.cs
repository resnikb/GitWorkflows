using System;

namespace GitWorkflows.Package.Common
{
    class EventArgs<T> : EventArgs
    {
        public T Value
        { get; private set; }

        public EventArgs(T value)
        { Value = value; }
    }
}