// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Presentation.Legacy
{
    public class AnonymousObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> onSubscribe;

        public AnonymousObservable(Func<IObserver<T>, IDisposable> onSubscribe)
        {
            if (onSubscribe == null)
                throw new ArgumentNullException("onSubscribe");

            this.onSubscribe = onSubscribe;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException("observer");

            IDisposable subsciption = onSubscribe(observer);
            if (subsciption == null)
                throw new InvalidOperationException("Subscription to an IObservable must produce a proper IDisposable instance.");

            return subsciption;
        }
    }
}
