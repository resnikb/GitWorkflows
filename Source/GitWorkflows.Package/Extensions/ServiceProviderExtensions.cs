using System;
using System.Runtime.InteropServices;
using GitWorkflows.Package.Common;
using Microsoft.VisualStudio;

namespace GitWorkflows.Package.Extensions
{
    static class ServiceProviderExtensions
    {
        private static object GetServiceChecked(IServiceProvider serviceProvider, Type type)
        {
            var result = TryGetService(serviceProvider, type);
            if (result == null)
                throw new ServiceNotFoundException(type.Name);

            return result;
        }
            
        public static T GetService<T>(this IServiceProvider serviceProvider)
        { return (T)GetServiceChecked(serviceProvider, typeof(T)); }
            
        public static object TryGetService(this IServiceProvider serviceProvider, Type type)
        {
            Arguments.EnsureNotNull(new {serviceProvider, type});
            return serviceProvider.GetService(type);
        }

        public static TServiceInterface GetService<TServiceClass, TServiceInterface>(this IServiceProvider serviceProvider) where TServiceInterface : class
        { return (TServiceInterface)(object)GetService<TServiceClass>(serviceProvider); }

        public static TServiceInterface TryGetService<TServiceClass, TServiceInterface>(this IServiceProvider serviceProvider) where TServiceInterface : class
        { return TryGetService<TServiceClass>(serviceProvider) as TServiceInterface; }

        public static TServiceInterface GetGlobalService<TServiceClass, TServiceInterface>(this IServiceProvider serviceProvider) where TServiceInterface : class
        {
            var result = TryGetGlobalService<TServiceClass, TServiceInterface>(serviceProvider);
            if (result == null)
                throw new ServiceNotFoundException(typeof(TServiceClass).Name);

            return result;
        }

        public static TServiceInterface TryGetGlobalService<TServiceClass, TServiceInterface>(this IServiceProvider serviceProvider) where TServiceInterface : class
        {
            Arguments.EnsureNotNull(new{serviceProvider});

            var oleProvider = GetService<Microsoft.VisualStudio.OLE.Interop.IServiceProvider>(serviceProvider);

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

        public static T TryGetService<T>(this IServiceProvider serviceProvider)
        { return (T)TryGetService(serviceProvider, typeof(T)); }    
    }
}