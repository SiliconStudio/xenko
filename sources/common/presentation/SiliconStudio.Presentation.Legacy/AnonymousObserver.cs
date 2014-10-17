// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Represent a proxy that route IObservable notifications to lambdas.
    /// </summary>
    /// <typeparam name="T">Type of the observed values.</typeparam>
    public class AnonymousObserver<T> : IObserver<T>
    {
        private readonly Action<T> onNext;
        private readonly Action onCompleted;
        private readonly Action<Exception> onError;

        /// <summary>
        /// Subscribes to OnNext notifications.
        /// </summary>
        /// <param name="onNext">Anonymous method that is called when IObserver.OnNext is called.</param>
        /// <remarks>For cases where you care only about data, like subscribing to a hot observable.</remarks>
        public AnonymousObserver(Action<T> onNext)
            : this(onNext, () => { }, _ => { })
        {
        }

        /// <summary>
        /// Subscribes to OnCompleted notifications.
        /// </summary>
        /// <param name="onCompleted">Anonymous method that is called when IObserver.OnCompleted is called.</param>
        /// <remarks>For cases where you care only about job completion,
        /// like awaiting for observable sequence to finish to run a new job.</remarks>
        public AnonymousObserver(Action onCompleted)
            : this(_ => { }, onCompleted, _ => { })
        {
        }

        /// <summary>
        /// Subscribes to OnError notifications.
        /// </summary>
        /// <param name="onError">Anonymous method that is called when IObserver.OnError is called.</param>
        /// <remarks>For cases where you care only about observable sequence ran into an error on not.</remarks>
        public AnonymousObserver(Action<Exception> onError)
            : this(_ => { }, () => { }, onError)
        {
        }

        /// <summary>
        /// Subscribes to OnNext and OnCompleted notifications.
        /// </summary>
        /// <param name="onNext">Anonymous method that is called when IObserver.OnNext is called.</param>
        /// <param name="onCompleted">Anonymous method that is called when IObserver.OnCompleted is called.</param>
        /// <remarks>For cases where you do not care about errors.</remarks>
        public AnonymousObserver(Action<T> onNext, Action onCompleted)
            : this(onNext, onCompleted, _ => { })
        {
        }

        /// <summary>
        /// Subscribes to OnNext and OnError notifications.
        /// </summary>
        /// <param name="onNext">Anonymous method that is called when IObserver.OnNext is called.</param>
        /// <param name="onError">Anonymous method that is called when IObserver.OnError is called.</param>
        /// <remarks>For cases where you do not care about completion,
        /// like subscribing to a hot observable that may throw an exception.</remarks>
        public AnonymousObserver(Action<T> onNext, Action<Exception> onError)
            : this(onNext, () => { }, onError)
        {
        }

        /// <summary>
        /// Subscribes to OnCompleted and OnError notifications.
        /// </summary>
        /// <param name="onCompleted">Anonymous method that is called when IObserver.OnCompleted is called.</param>
        /// <param name="onError">Anonymous method that is called when IObserver.OnError is called.</param>
        /// <remarks>For cases where you do not care about data and just about completion (succeeded of failed),
        /// like awaiting for observable sequence to finish (no matter how) to run a new job.</remarks>
        public AnonymousObserver(Action onCompleted, Action<Exception> onError)
            : this(_ => { }, onCompleted, onError)
        {
        }

        /// <summary>
        /// Subscribes to OnNext, OnCompleted and OnError notifications.
        /// </summary>
        /// <param name="onNext">Anonymous method that is called when IObserver.OnNext is called.</param>
        /// <param name="onCompleted">Anonymous method that is called when IObserver.OnCompleted is called.</param>
        /// <param name="onError">Anonymous method that is called when IObserver.OnError is called.</param>
        public AnonymousObserver(Action<T> onNext, Action onCompleted, Action<Exception> onError)
        {
            if (onNext == null)
                throw new ArgumentNullException("onNext");

            if (onCompleted == null)
                throw new ArgumentNullException("onCompleted");

            if (onError == null)
                throw new ArgumentNullException("onError");

            this.onNext = onNext;
            this.onCompleted = onCompleted;
            this.onError = onError;
        }

        public void OnNext(T value)
        {
            onNext(value);
        }

        public void OnCompleted()
        {
            onCompleted();
        }

        public void OnError(Exception error)
        {
            onError(error);
        }
    }
}
