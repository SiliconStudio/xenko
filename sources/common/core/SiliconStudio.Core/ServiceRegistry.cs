// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using System.Reflection;
using SiliconStudio.Core.Annotations;

// THIS NAMESPACE MUST BE USED FOR 4.5 CORE PROFILE

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base implementation for <see cref="IServiceRegistry"/>
    /// </summary>
    public class ServiceRegistry : IServiceRegistry
    {
        public static PropertyKey<IServiceRegistry> ServiceRegistryKey = new PropertyKey<IServiceRegistry>("ServiceRegistryKey", typeof(IServiceRegistry));

        private readonly Dictionary<Type, object> registeredService = new Dictionary<Type, object>();

        #region IServiceRegistry Members

        /// <summary>
        /// Gets the instance service providing a specified service.
        /// </summary>
        /// <param name="type">The type of service.</param>
        /// <returns>The registered instance of this service.</returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public object GetService([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            lock (registeredService)
            {
                if (registeredService.ContainsKey(type))
                    return registeredService[type];
            }

            return null;
        }

        public event EventHandler<ServiceEventArgs> ServiceAdded;

        public event EventHandler<ServiceEventArgs> ServiceRemoved;

        /// <summary>
        /// Adds a service to this <see cref="ServiceRegistry"/>.
        /// </summary>
        /// <param name="type">The type of service to add.</param>
        /// <param name="provider">The service provider to add.</param>
        /// <exception cref="System.ArgumentNullException">type;Service type cannot be null</exception>
        /// <exception cref="System.ArgumentException">Service is already registered;type</exception>
        public void AddService([NotNull] Type type, [NotNull] object provider)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (!type.GetTypeInfo().IsAssignableFrom(provider.GetType().GetTypeInfo()))
                throw new ArgumentException($"Service [{provider.GetType().FullName}] must be assignable to [{type.FullName}]");

            lock (registeredService)
            {
                if (registeredService.ContainsKey(type))
                    throw new ArgumentException("Service is already registered", nameof(type));
                registeredService.Add(type, provider);
            }
            OnServiceAdded(new ServiceEventArgs(type, provider));
        }

        /// <summary>Removes the object providing a specified service.</summary>
        /// <param name="type">The type of service.</param>
        public void RemoveService([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            object oldService = null;
            lock (registeredService)
            {
                if (registeredService.TryGetValue(type, out oldService))
                    registeredService.Remove(type);
            }
            if (oldService != null)
                OnServiceRemoved(new ServiceEventArgs(type, oldService));
        }

        #endregion

        private void OnServiceAdded(ServiceEventArgs e)
        {
            ServiceAdded?.Invoke(this, e);
        }

        private void OnServiceRemoved(ServiceEventArgs e)
        {
            ServiceRemoved?.Invoke(this, e);
        }
    }
}
