// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Operators
{
    internal sealed class EmptyObservable<T> : OperatorObservableBase<T>
    {
        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            observer = new Empty(observer, cancel);
            observer.OnCompleted();
            return Disposable.Empty;
        }

        private sealed class Empty : OperatorObserverBase<T, T>
        {
            public Empty(IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
            }

            public override void OnNext(T value)
            {
                try
                {
                    Observer.OnNext(value);
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public override void OnError(Exception error)
            {
                try
                {
                    Observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    Observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }

    internal class ImmutableEmptyObservable<T> : IObservable<T>
    {
        internal static readonly ImmutableEmptyObservable<T> Instance = new();

        private ImmutableEmptyObservable()
        {
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
}