// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Core
{
    /// <summary>
    /// Base class for a framework component.
    /// </summary>
    public abstract class ComponentBase : IComponent, IDisposable, ICollectorHolder, IReferencable
    {
        private static long globalCounterId;
        private int counter = 1;
        private ObjectCollector collector;
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBase"/> class.
        /// </summary>
        protected ComponentBase()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentBase"/> class.
        /// </summary>
        /// <param name="name">The name attached to this component</param>
        protected ComponentBase(string name)
        {
            Name = name ?? GetType().Name;
            Id = Interlocked.Increment(ref globalCounterId);
            collector = new ObjectCollector();

            // Track this component
            if (ComponentTracker.Enable) ComponentTracker.Track(this);
            Tags = new PropertyContainer(this);
        }

        public long Id { get; private set; }

        /// <inheritdoc/>
        int IReferencable.ReferenceCount { get { return counter; } }

        /// <summary>
        /// Gets or sets the name of this component.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value == name) return;

                name = value;
                OnNameChanged();
            }
        }

        /// <summary>
        /// Called when <see cref="Name"/> property was changed.
        /// </summary>
        protected virtual void OnNameChanged()
        {
        }

        /// <inheritdoc/>
        int IReferencable.AddReference()
        {
            if (ComponentTracker.Enable && ComponentTracker.EnableEvents)
                ComponentTracker.NotifyEvent(this, ComponentEventType.AddReference);

            int newCounter = Interlocked.Increment(ref counter);
            if (newCounter <= 1) throw new InvalidOperationException(FrameworkResources.AddReferenceError);
            return newCounter;
        }

        /// <inheritdoc/>
        int IReferencable.Release()
        {
            if (ComponentTracker.Enable && ComponentTracker.EnableEvents)
                ComponentTracker.NotifyEvent(this, ComponentEventType.Release);

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
            // Untrack this object
            if (ComponentTracker.Enable) ComponentTracker.UnTrack(this);

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

        public override string ToString()
        {
            return string.Format("{0}: {1}", this.GetType().Name, Name);
        }

        /// <summary>
        /// Gets the attached properties to this component.
        /// </summary>
        public PropertyContainer Tags;
    }
}