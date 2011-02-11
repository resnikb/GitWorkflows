using System;

namespace GitWorkflows.Services
{
    public class ViewNotFoundException : Exception
    {
        public ViewNotFoundException(string viewModelName)
            : base("Cannot find control to host view model "+viewModelName)
        {}

        public ViewNotFoundException(string viewModelName, Exception innerException)
            : base("Cannot find control to host view model "+viewModelName, innerException)
        {}
    }
}