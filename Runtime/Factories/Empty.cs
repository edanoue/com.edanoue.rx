// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx
{
    public static partial class Observable
    {
        public static Observable<T> Empty<T>()
        {
            return Rx.Empty<T>.Instance;
        }
    }

    internal sealed class Empty<T> : Observable<T>
    {
        // singleton
        public static readonly Empty<T> Instance = new();

        private Empty()
        {
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}