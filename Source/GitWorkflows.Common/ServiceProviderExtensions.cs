using System;

namespace GitWorkflows.Common
{
    public static class ServiceProviderExtensions
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

        public static T TryGetService<T>(this IServiceProvider serviceProvider)
        { return (T)TryGetService(serviceProvider, typeof(T)); }    
    }
}