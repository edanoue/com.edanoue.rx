// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Edanoue.Rx.Internal;

namespace Edanoue.Rx.Operators
{
    internal abstract class OperatorObserverBase<TSource, TResult> : IDisposable, IObserver<TSource>
    {
        private   IDisposable?       _cancel;
        protected IObserver<TResult> Observer;

        protected OperatorObserverBase(IObserver<TResult> observer, IDisposable cancel)
        {
            Observer = observer;
            _cancel = cancel;
        }

        public void Dispose()
        {
            Observer = NoOpObserver<TResult>.Default;
            var target = Interlocked.Exchange(ref _cancel, null);
            target?.Dispose();
        }

        public abstract void OnNext(TSource value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();
    }
}