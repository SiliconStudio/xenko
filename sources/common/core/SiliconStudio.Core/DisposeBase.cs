// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base class for a <see cref="IDisposable"/> interface.
    /// </summary>
    [DataContract]
    public abstract class DisposeBase : IDisposable, ICollectorHolder, IReferencable
    {
        private int counter = 1;
        private ObjectCollector collector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBase"/> class.
        /// </summary>
        protected DisposeBase()
        {
            collector = new ObjectCollector();
            Tags = new PropertyContainer(this);
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                int newcounter = Interlocked.Decrement(ref counter);
                if (newcounter != 0)
                    throw new InvalidOperationException(FrameworkResources.ReleaseReferenceError);
                Destroy();
            }
            IsDisposed = true;
        }

        /// <summary>
        /// Has the component been disposed or not yet.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes of object resources.
        /// </summary>
        protected virtual void Destroy()
        {
            collector.Dispose();
        }

        ObjectCollector ICollectorHolder.Collector
        {
            get
            {
                collector.EnsureValid();
                return collector;
            }
        }

        /// <summary>
        /// Gets the attached properties to this component.
        /// </summary>
        [DataMemberIgnore] // Do not try to recreate object (preserve Tags.Owner)
        public PropertyContainer Tags;

        /// <inheritdoc/>
        int IReferencable.ReferenceCount { get { return counter; } }

        /// <inheritdoc/>
        int IReferencable.AddReference()
        {
            OnAddReference();

            int newCounter = Interlocked.Increment(ref counter);
            if (newCounter <= 1) throw new InvalidOperationException(FrameworkResources.AddReferenceError);
            return newCounter;
        }

        /// <inheritdoc/>
        int IReferencable.Release()
        {
            OnReleaseReference();

            int newCounter = Interlocked.Decrement(ref counter);
            if (newCounter == 0)
            {
                Destroy();
                IsDisposed = true;
            }
            else if (newCounter < 0)
            {
                throw new InvalidOperationException(FrameworkResources.ReleaseReferenceError);
            }
            return newCounter;
        }

        protected virtual void OnAddReference()
        {
        }

        protected virtual void OnReleaseReference()
        {
        }
    }
}