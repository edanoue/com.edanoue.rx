// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx.Operators
{
    internal class SkipObservable<T> : OperatorObservableBase<T>
    {
        private readonly int            _count;
        private readonly IObservable<T> _source;

        public SkipObservable(IObservable<T> source, int count)
        {
            _source = source;
            _count = count;
        }

        // optimize for .Skip().Skip()
        public IObservable<T> Combine(int count)
        {
            // use sum
            // xs = 6
            // xs.Skip(2) = 4
            // xs.Skip(2).Skip(3) = 1

            return new SkipObservable<T>(_source, _count + count);
        }

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            return _source.Subscribe(new Skip(this, observer, cancel));
        }

        private class Skip : OperatorObserverBase<T, T>
        {
            private int _remaining;

            public Skip(SkipObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                _remaining = parent._count;
            }

            public override void OnNext(T value)
            {
                if (_remaining <= 0)
                {
                    Observer.OnNext(value);
                }
                else
                {
                    _remaining--;
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