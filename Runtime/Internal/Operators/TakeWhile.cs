// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx.Operators
{
    internal sealed class TakeWhileObservable<T> : OperatorObservableBase<T>
    {
        private readonly Func<T, bool>?      _predicateNoIndex;
        private readonly Func<T, int, bool>? _predicateWithIndex;
        private readonly IObservable<T>      _source;

        public TakeWhileObservable(IObservable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicateNoIndex = predicate;
        }

        public TakeWhileObservable(IObservable<T> source, Func<T, int, bool> predicateWithIndex)
        {
            _source = source;
            _predicateWithIndex = predicateWithIndex;
        }

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            if (_predicateNoIndex is not null)
            {
                return new TakeWhileNoIndex(this, observer, cancel).Run();
            }

            return new TakeWhileWithIndex(this, observer, cancel).Run();
        }

        private sealed class TakeWhileNoIndex : OperatorObserverBase<T, T>
        {
            private readonly TakeWhileObservable<T> _parent;

            public TakeWhileNoIndex(TakeWhileObservable<T> parent, IObserver<T> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            public IDisposable Run()
            {
                return _parent._source.Subscribe(this);
            }

            public override void OnNext(T value)
            {
                bool isPassed;
                try
                {
                    isPassed = _parent._predicateNoIndex!(value);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Observer.OnError(ex);
                    }
                    finally
                    {
                        Dispose();
                    }

                    return;
                }

                if (isPassed)
                {
                    Observer.OnNext(value);
                }
                else
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

        private class TakeWhileWithIndex : OperatorObserverBase<T, T>
        {
            private readonly TakeWhileObservable<T> _parent;
            private          int                    _index;

            public TakeWhileWithIndex(TakeWhileObservable<T> parent, IObserver<T> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
            }

            public IDisposable Run()
            {
                return _parent._source.Subscribe(this);
            }

            public override void OnNext(T value)
            {
                bool isPassed;
                try
                {
                    isPassed = _parent._predicateWithIndex!(value, _index++);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Observer.OnError(ex);
                    }
                    finally
                    {
                        Dispose();
                    }

                    return;
                }

                if (isPassed)
                {
                    Observer.OnNext(value);
                }
                else
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