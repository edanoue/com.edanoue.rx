// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx.Operators
{
    internal sealed class TakeObservable<T> : OperatorObservableBase<T>
    {
        private readonly int            _count;
        private readonly IObservable<T> _source;

        public TakeObservable(IObservable<T> source, int count)
        {
            _source = source;
            _count = count;
        }

        // optimize combiner

        public IObservable<T> Combine(int count)
        {
            // xs = 6
            // xs.Take(5) = 5         | xs.Take(3) = 3
            // xs.Take(5).Take(3) = 3 | xs.Take(3).Take(5) = 3

            // use minimum one
            return _count <= count
                ? this
                : new TakeObservable<T>(_source, count);
        }

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            return _source.Subscribe(new Take(this, observer, cancel));
        }

        private sealed class Take : OperatorObserverBase<T, T>
        {
            private int _rest;

            public Take(TakeObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                _rest = parent._count;
            }

            public override void OnNext(T value)
            {
                if (_rest <= 0)
                {
                    return;
                }

                _rest -= 1;
                Observer.OnNext(value);

                if (_rest > 0)
                {
                    return;
                }

                try
                {
                    Observer.OnCompleted();
                }
                finally
                {
                    Dispose();
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
}