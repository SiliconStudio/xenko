// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Represent observable, as weel as a collection of subscribed observers.
    /// This class purpose is to help constructing observable object with multiple observer subscriptions.
    /// </summary>
    /// <typeparam name="T">Type of the observable values.</typeparam>
    public class ObserverContainer<T> : IObservable<T>, IEnumerable<IObserver<T>>
    {
        /// <summary>
        /// Gets th synchronization root for the observers.
        /// It must be locked when iterating over the Observers property to ensure thread safety.
        /// </summary>
        public object SyncRoot { get { return syncRoot; } }
        /// <summary>
        /// Gets the collection of subscribed observers.
        /// </summary>
        public ReadOnlyCollection<IObserver<T>> Observers { get; private set; }

        private readonly object syncRoot = new object();
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();

        public ObserverContainer()
        {
            Observers = new ReadOnlyCollection<IObserver<T>>(observers);
        }

        /// <summary>
        /// Subscribes an observer.
        /// </summary>
        /// <param name="observer">Observer to keep an eye on.</param>
        /// <returns>Returns the subscription token that perform unsubscription when disposed.</returns>
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            lock (syncRoot)
            {
                observers.Add(observer);
            }

            return new InternalDisposer(syncRoot, observer, observers);
        }

        public IEnumerator<IObserver<T>> GetEnumerator()
        {
            return Observers.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Observers.GetEnumerator();
        }

        /// <summary>
        /// Internal disposed that detach a given observer from the ObserverContainer.
        /// </summary>
        private class InternalDisposer : IDisposable
        {
            private object syncRoot;
            private IObserver<T> observer;
            private List<IObserver<T>> observers;

            public InternalDisposer(object syncRoot, IObserver<T> observer, List<IObserver<T>> observers)
            {
                this.syncRoot = syncRoot;
                this.observer = observer;
                this.observers = observers;
            }

            public void Dispose()
            {
                if (observer == null)
                    return;

                lock (syncRoot)
                {
                    observers.Remove(observer);
                }

                syncRoot = null;
                observer = null;
                observers = null;
            }
        }
    }
}
