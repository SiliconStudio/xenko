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
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core
{
    /// <summary>
    /// A service registry is a <see cref="IServiceProvider"/> that provides methods to register and unregister services.
    /// </summary>
    public interface IServiceRegistry
    {
        /// <summary>
        /// Occurs when a new service is added.
        /// </summary>
        event EventHandler<ServiceEventArgs> ServiceAdded;

        /// <summary>
        /// Occurs when when a service is removed.
        /// </summary>
        event EventHandler<ServiceEventArgs> ServiceRemoved;

        /// <summary>
        /// Adds a service to this <see cref="ServiceRegistry"/>.
        /// </summary>
        /// <typeparam name="T">The type of service to add.</typeparam>
        /// <param name="service">The service to add.</param>
        /// <exception cref="ArgumentNullException">Service cannot be null</exception>
        /// <exception cref="ArgumentException">Service is already registered with this type</exception>
        void AddService<T>([NotNull] T service) where T : class;

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the service to retrieve.</typeparam>
        /// <returns>A service of the requested type, or [null] if not found.</returns>
        [CanBeNull]
        T GetService<T>() where T : class;

        /// <summary>
        /// Removes the object providing a specified service.
        /// </summary>
        /// <typeparam name="T">The type of the service to remove.</typeparam>
        void RemoveService<T>() where T : class;

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">The type of the service to get.</param>
        /// <returns>A service of the requested type, or [null] if not found.</returns>
        [Obsolete("Use the type-safe generic overload instead")]
        object GetService(Type serviceType);

        /// <summary>
        /// Adds a service to this service provider.
        /// </summary>
        /// <param name="type">The type of service to add.</param>
        /// <param name="service">The instance of the service to add.</param>
        /// <exception cref="ArgumentNullException">Service type cannot be null</exception>
        /// <exception cref="ArgumentException">Service is already registered</exception>
        [Obsolete("Use the type-safe  generic overload instead")]
        void AddService(Type type, object service);

        /// <summary>
        /// Removes the object providing a specified service.
        /// </summary>
        /// <param name="type">The type of service.</param>
        [Obsolete("Use the type-safe  generic overload instead")]
        void RemoveService(Type type);
    }
}
