using System;
using System.Runtime.InteropServices;
using GitWorkflows.Common;
using Microsoft.VisualStudio;

namespace GitWorkflows.Package
{
    static class ServiceProviderExtensions
    {
        public static TServiceInterface TryGetGlobalService<TServiceClass, TServiceInterface>(this IServiceProvider serviceProvider) where TServiceInterface : class
        {
            Arguments.EnsureNotNull(new{serviceProvider});

            var oleProvider = serviceProvider.GetService<Microsoft.VisualStudio.OLE.Interop.IServiceProvider>();

            var serviceGuid = typeof(TServiceClass).GUID;
            var interfaceGuid = typeof(TServiceInterface).GUID;
            var obj = IntPtr.Zero;

            var result = ErrorHandler.CallWithCOMConvention(() => oleProvider.QueryService(ref serviceGuid, ref interfaceGuid, out obj));
            if (ErrorHandler.Failed(result) || obj == IntPtr.Zero)
                return null;

            try
            {
                return (TServiceInterface)Marshal.GetObjectForIUnknown(obj);
            }
            finally
            {
                Marshal.Release(obj);
            }
        }

        public static TServiceInterface GetGlobalService<TServiceClass, TServiceInterface>(this IServiceProvider serviceProvider) where TServiceInterface : class
        {
            var result = TryGetGlobalService<TServiceClass, TServiceInterface>(serviceProvider);
            if (result == null)
                throw new ServiceNotFoundException(typeof(TServiceClass).Name);

            return result;
        }

    }
}