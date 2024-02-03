// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Operators
{
    internal class WhereObservable<T> : OperatorObservableBase<T>
    {
        private readonly Func<T, bool>?      _predicateNoIndex;
        private readonly Func<T, int, bool>? _predicateWithIndex;
        private readonly IObservable<T>      _source;

        public WhereObservable(IObservable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicateNoIndex = predicate;
        }

        public WhereObservable(IObservable<T> source, Func<T, int, bool> predicateWithIndex)
        {
            _source = source;
            _predicateWithIndex = predicateWithIndex;
        }

        // Optimize for .Where().Where()
        public IObservable<T> CombinePredicate(Func<T, bool> combinePredicate)
        {
            if (_predicateNoIndex is not null)
            {
                return new WhereObservable<T>(_source, x => _predicateNoIndex(x) && combinePredicate(x));
            }

            return new WhereObservable<T>(this, combinePredicate);
        }

        /*
        // Optimize for .Where().Select()
        public IObservable<TR> CombineSelector<TR>(Func<T, TR> selector)
        {
            if (this._predicate != null)
            {
                return new WhereSelectObservable<T, TR>(_source, _predicate, selector);
            }
            else
            {
                return new SelectObservable<T, TR>(this, selector); // can't combine
            }
        }
        */

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            if (_predicateNoIndex is not null)
            {
                return _source.Subscribe(new WhereNoIndex(this, observer, cancel));
            }

            return _source.Subscribe(new WhereWithIndex(this, observer, cancel));
        }

        private class WhereNoIndex : OperatorObserverBase<T, T>
        {
            private readonly WhereObservable<T> _parent;

            public WhereNoIndex(WhereObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(
                observer, cancel)
            {
                _parent = parent;
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

        private class WhereWithIndex : OperatorObserverBase<T, T>
        {
            private readonly WhereObservable<T> _parent;
            private          int                _index;

            public WhereWithIndex(WhereObservable<T> parent, IObserver<T> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                _parent = parent;
                _index = 0;
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