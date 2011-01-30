using System;
using GitWorkflows.Package.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using NLog.Targets;

namespace GitWorkflows.Package.Logging
{
    [Target("VsOutputPaneTarget")]
    public class VsOutputPaneTarget : TargetWithLayout
    {
        private IVsOutputWindowPane _pane;
        private Guid _guid;

        internal static IServiceProvider ServiceProvider
        { get; set; }

        public string PaneName
        { get; set; }

        public string Guid
        { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            
            if (string.IsNullOrEmpty(Guid))
                _guid = System.Guid.NewGuid();
            else
                _guid = System.Guid.ParseExact(Guid, "D");
        }

        private bool TryInitializePane()
        {
            if (_pane != null)
                return true;

            var outputWindow = ServiceProvider.TryGetGlobalService<SVsOutputWindow, IVsOutputWindow>();
            if (outputWindow == null)
                return false;

            if ( ErrorHandler.Succeeded(ErrorHandler.CallWithCOMConvention(() => outputWindow.GetPane(ref _guid, out _pane))) && _pane != null )
                return true;

            if ( ErrorHandler.Failed(ErrorHandler.CallWithCOMConvention(() => outputWindow.CreatePane(ref _guid, PaneName, 1, 0))) )
                return false;
            
            return ErrorHandler.Succeeded(ErrorHandler.CallWithCOMConvention(() => outputWindow.GetPane(ref _guid, out _pane)));
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (TryInitializePane())
            {
                var output = Layout.Render(logEvent);
                _pane.OutputStringThreadSafe(output + Environment.NewLine);
            }
        }
    }
}