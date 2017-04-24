// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core
{
    public static class ServiceRegistryExtensions
    {
        /// <summary>
        /// Gets a service instance from a specified interface contract.
        /// </summary>
        /// <typeparam name="T">Type of the interface contract of the service</typeparam>
        /// <param name="registry">The registry.</param>
        /// <returns>An instance of the requested service registered to this registry.</returns>
        public static T GetServiceAs<T>([NotNull] this IServiceRegistry registry)
        {
            return (T)registry.GetService(typeof(T));
        }

        /// <summary>
        /// Gets a service instance from a specified interface contract.
        /// </summary>
        /// <typeparam name="T">Type of the interface contract of the service</typeparam>
        /// <param name="registry">The registry.</param>
        /// <exception cref="ServiceNotFoundException">If the service was not found</exception>
        /// <returns>An instance of the requested service registered to this registry.</returns>
        public static T GetSafeServiceAs<T>([NotNull] this IServiceRegistry registry)
        {
            var serviceFound = (T)registry.GetService(typeof(T));
            if (Equals(serviceFound, default(T)))
            {
                throw new ServiceNotFoundException(typeof(T));
            }
            return serviceFound;
        }

        /// <summary>
        /// Gets a service instance from a specified interface contract.
        /// </summary>
        /// <typeparam name="T">Type of the interface contract of the service</typeparam>
        /// <param name="registry">The registry.</param>
        /// <param name="serviceReady">The service ready.</param>
        /// <returns>An instance of the requested service registered to this registry.</returns>
        /// <exception cref="ServiceNotFoundException">If the service was not found</exception>
        public static void GetServiceLate<T>([NotNull] this IServiceRegistry registry, Action<T> serviceReady)
        {
            var instance = GetServiceAs<T>(registry);
            if (Equals(instance, null))
            {
                var deferred = new ServiceDeferredRegister<T>(registry, serviceReady);
                deferred.Register();
            }
            else
            {
                serviceReady(instance);
            }
        }    

        private class ServiceDeferredRegister<T>
        {
            private readonly IServiceRegistry services;
            private readonly Action<T> serviceReady;

            public ServiceDeferredRegister(IServiceRegistry registry, Action<T> serviceReady)
            {
                services = registry;
                this.serviceReady = serviceReady;
            }

            public void Register()
            {
                services.ServiceAdded += Services_ServiceAdded;
            }

            private void Services_ServiceAdded(object sender, [NotNull] ServiceEventArgs args)
            {
                if (args.ServiceType == typeof(T))
                {
                    serviceReady((T)args.Instance);
                    services.ServiceAdded -= Services_ServiceAdded;
                }
            }
        }
    }
}
