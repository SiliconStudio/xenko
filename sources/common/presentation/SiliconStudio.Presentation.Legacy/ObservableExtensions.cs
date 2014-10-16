// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading;

namespace SiliconStudio.Presentation.Legacy
{
    public static class ObservableExtensions
    {
        public static IObservable<T> ObserveOn<T>(this IObservable<T> source, SynchronizationContext context)
        {
            return new AnonymousObservable<T>(observer =>
            {
                var delegateObserver = new AnonymousObserver<T>(
                    value => context.Post(_ => observer.OnNext(value), null),
                    () => context.Post(_ => observer.OnCompleted(), null),
                    error => context.Post(_ => observer.OnError(error), null));

                return source.Subscribe(delegateObserver);
            });
        }

        /// <summary>
        /// Subscribes an onNext anonymous method to the OnNext notifications of the observable.
        /// </summary>
        /// <typeparam name="T">Type of the items in the observable sequence.</typeparam>
        /// <param name="source">Observable to subscribe on.</param>
        /// <param name="onNext">Method called when a data is available in the observable sequence.</param>
        /// <returns>Returns the subscription token that perform unsubscription when disposed.</returns>
        /// <see cref="AnonymousObserver{T}"/>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext));
        }

        /// <summary>
        /// Subscribes an onNext and onCompleted anonymous methods to the OnNext and OnCompleted (respectively) notifications of the observable.
        /// </summary>
        /// <typeparam name="T">Type of the items in the observable sequence.</typeparam>
        /// <param name="source">Observable to subscribe on.</param>
        /// <param name="onNext">Method called when a data is available in the observable sequence.</param>
        /// <param name="onCompleted">Method called when a observable sequence is completed.</param>
        /// <returns>Returns the subscription token that perform unsubscription when disposed.</returns>
        /// <see cref="AnonymousObserver{T}"/>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext, onCompleted));
        }

        /// <summary>
        /// Subscribes an onNext and onError anonymous methods to the OnNext and OnError (respectively) notifications of the observable.
        /// </summary>
        /// <typeparam name="T">Type of the items in the observable sequence.</typeparam>
        /// <param name="source">Observable to subscribe on.</param>
        /// <param name="onNext">Method called when a data is available in the observable sequence.</param>
        /// <param name="onError">Method called when a observable sequence run into an error.</param>
        /// <returns>Returns the subscription token that perform unsubscription when disposed.</returns>
        /// <see cref="AnonymousObserver{T}"/>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext, onError));
        }

        /// <summary>
        /// Subscribes an onCompleted and onError anonymous methods to the OnCompleted and OnError (respectively) notifications of the observable.
        /// </summary>
        /// <typeparam name="T">Type of the items in the observable sequence.</typeparam>
        /// <param name="source">Observable to subscribe on.</param>
        /// <param name="onCompleted">Method called when a observable sequence is completed.</param>
        /// <param name="onError">Method called when a observable sequence run into an error.</param>
        /// <returns>Returns the subscription token that perform unsubscription when disposed.</returns>
        /// <see cref="AnonymousObserver{T}"/>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action onCompleted, Action<Exception> onError)
        {
            return source.Subscribe(new AnonymousObserver<T>(onCompleted, onError));
        }

        /// <summary>
        /// Subscribes an onNext, onCompleted and onError anonymous methods to the OnNext, OnCompleted and OnError (respectively) notifications of the observable.
        /// </summary>
        /// <typeparam name="T">Type of the items in the observable sequence.</typeparam>
        /// <param name="source">Observable to subscribe on.</param>
        /// <param name="onNext">Method called when a data is available in the observable sequence.</param>
        /// <param name="onCompleted">Method called when a observable sequence is completed.</param>
        /// <param name="onError">Method called when a observable sequence run into an error.</param>
        /// <returns>Returns the subscription token that perform unsubscription when disposed.</returns>
        /// <see cref="AnonymousObserver{T}"/>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted, Action<Exception> onError)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext, onCompleted, onError));
        }
    }
}
