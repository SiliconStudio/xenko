// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// A service provider class for view model objects.
    /// </summary>
    public class ViewModelServiceProvider : IViewModelServiceProvider
    {
        private readonly IViewModelServiceProvider parentProvider;
        private readonly List<object> services = new List<object>();

        public static IViewModelServiceProvider NullServiceProvider = new ViewModelServiceProvider(Enumerable.Empty<object>());

        public ViewModelServiceProvider(IEnumerable<object> services)
        {
            foreach (var service in services)
            {
                RegisterService(service);
            }
        }

        public ViewModelServiceProvider(IViewModelServiceProvider parentProvider, IEnumerable<object> services)
            : this(services)
        {
            // If the parent provider is a ViewModelServiceProvider, try to merge its service list instead of using composition.
            var parent = parentProvider as ViewModelServiceProvider;
            if (parent != null)
            {
                parent.services.ForEach(RegisterService);
            }
            else
            {
                this.parentProvider = parentProvider;
            }
        }

        /// <summary>
        /// Register a new service in this <see cref="ViewModelServiceProvider"/>.
        /// </summary>
        /// <param name="service">The service to register.</param>
        /// <exception cref="InvalidOperationException">A service of the same type has already been registered.</exception>
        protected void RegisterService(object service)
        {
            if (service == null) throw new ArgumentNullException("service");
            if (services.Any(x => x.GetType() == service.GetType()))
                throw new InvalidOperationException("A service of the same type has already been registered.");

            services.Add(service);
        }

        public void UnregisterService(object service)
        {
            if (service == null) throw new ArgumentNullException("service");

            services.Remove(service);
        }

        /// <inheritdoc/>
        public object TryGet(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            object serviceFound = null;

            foreach (var service in services.Where(serviceType.IsInstanceOfType))
            {
                if (serviceFound != null)
                    throw new InvalidOperationException("Multiple services match the given type.");

                serviceFound = service;
            }

            return parentProvider != null && serviceFound == null ? parentProvider.TryGet(serviceType) : serviceFound;
        }

        /// <inheritdoc/>
        public T TryGet<T>() where T : class
        {
            return (T)TryGet(typeof(T));
        }

        /// <inheritdoc/>
        public object Get(Type serviceType)
        {
            var result = TryGet(serviceType);
            if (result == null) throw new InvalidOperationException("No service matches the given type.");
            return result;
        }

        /// <inheritdoc/>
        public T Get<T>() where T : class
        {
            var result = (T)TryGet(typeof(T));
            if (result == null) throw new InvalidOperationException("No service matches the given type.");
            return result;
        }
    }
}