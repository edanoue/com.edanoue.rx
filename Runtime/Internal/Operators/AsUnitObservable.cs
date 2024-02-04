// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Operators
{
    internal sealed class AsUnitObservableObservable<T> : OperatorObservableBase<Unit>
    {
        private readonly IObservable<T> _source;

        public AsUnitObservableObservable(IObservable<T> source)
        {
            _source = source;
        }

        protected override IDisposable SubscribeInternal(IObserver<Unit> observer, IDisposable cancel)
        {
            return _source.Subscribe(new AsUnitObservable(observer, cancel));
        }

        private sealed class AsUnitObservable : OperatorObserverBase<T, Unit>
        {
            public AsUnitObservable(IObserver<Unit> observer, IDisposable cancel) : base(observer, cancel)
            {
            }

            public override void OnNext(T value)
            {
                Observer.OnNext(Unit.Default);
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
}