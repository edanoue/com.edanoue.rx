// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Operators
{
    internal class WhereObservable<T> : OperatorObservableBase<T>
    {
        private readonly Func<T, bool>  _predicate;
        private readonly IObservable<T> _source;

        public WhereObservable(IObservable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        // Optimize for .Where().Where()
        public IObservable<T> CombinePredicate(Func<T, bool> combinePredicate)
        {
            return new WhereObservable<T>(_source, x => _predicate(x) && combinePredicate(x));
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
            return _source.Subscribe(new Where(this, observer, cancel));
        }

        private class Where : OperatorObserverBase<T, T>
        {
            private readonly WhereObservable<T> _parent;

            public Where(WhereObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(
                observer, cancel)
            {
                _parent = parent;
            }

            public override void OnNext(T value)
            {
                bool isPassed;
                try
                {
                    isPassed = _parent._predicate(value);
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