using System;

namespace SiliconStudio.Presentation.ViewModel
{
    /// <summary>
    /// A service provider that is empty and immutable.
    /// </summary>
    internal class NullServiceProvider : IViewModelServiceProvider
    {
        /// <inheritdoc/>
        public event EventHandler<ServiceRegistrationEventArgs> ServiceRegistered;

        /// <inheritdoc/>
        public event EventHandler<ServiceRegistrationEventArgs> ServiceUnregistered;

        /// <inheritdoc/>
        public void RegisterService(object service)
        {
            throw new InvalidOperationException("Cannot register a service on a NullServiceProvider.");
        }

        /// <inheritdoc/>
        public void UnregisterService(object service)
        {
            throw new InvalidOperationException("Cannot unregister a service on a NullServiceProvider.");
        }

        /// <inheritdoc/>
        public object TryGet(Type serviceType)
        {
            return null;
        }

        /// <inheritdoc/>
        public T TryGet<T>() where T : class
        {
            return null;
        }

        /// <inheritdoc/>
        public object Get(Type serviceType)
        {
            throw new InvalidOperationException("No service matches the given type.");
        }

        /// <inheritdoc/>
        public T Get<T>() where T : class
        {
            throw new InvalidOperationException("No service matches the given type.");
        }
    }
}