using System;

namespace GitWorkflows.Common
{
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException(string name) : base(string.Format("Service not found '{0}'", name ?? "(null)"))
        {}
    }
}