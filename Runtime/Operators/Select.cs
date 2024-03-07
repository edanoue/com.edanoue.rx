// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, TResult> selector)
        {
            if (source is Where<T> where)
            {
                // Optimize for WhereSelect
                return new WhereSelect<T, TResult>(where.Source, selector, where.Predicate);
            }

            return new Select<T, TResult>(source, selector);
        }

        public static Observable<TResult> Select<T, TResult>(this Observable<T> source, Func<T, int, TResult> selector)
        {
            return new SelectIndexed<T, TResult>(source, selector);
        }

        public static Observable<TResult> Select<T, TResult, TState>(this Observable<T> source, TState state,
            Func<T, TState, TResult> selector)
        {
            return new Select<T, TResult, TState>(source, selector, state);
        }

        public static Observable<TResult> Select<T, TResult, TState>(this Observable<T> source, TState state,
            Func<T, int, TState, TResult> selector)
        {
            return new SelectIndexed<T, TResult, TState>(source, selector, state);
        }
    }

    internal sealed class Select<T, TResult> : Observable<TResult>
    {
        private readonly Func<T, TResult> _selector;
        private readonly Observable<T>    _source;

        public Select(Observable<T> source, Func<T, TResult> selector)
        {
            _source = source;
            _selector = selector;
        }

        protected override IDisposable SubscribeCore(Observer<TResult> observer)
        {
            return _source.Subscribe(new SelectObserver(observer, _selector));
        }

        private sealed class SelectObserver : Observer<T>
        {
            private readonly Observer<TResult> _observer;
            private readonly Func<T, TResult>  _selector;

            public SelectObserver(Observer<TResult> observer, Func<T, TResult> selector)
            {
                _observer = observer;
                _selector = selector;
            }

            protected override void OnNextCore(T value)
            {
                _observer.OnNext(_selector(value));
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                _observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                _observer.OnCompleted(result);
            }
        }
    }

    internal sealed class Select<T, TResult, TState> : Observable<TResult>
    {
        private readonly Func<T, TState, TResult> _selector;
        private readonly Observable<T>            _source;
        private readonly TState                   _state;

        public Select(Observable<T> source, Func<T, TState, TResult> selector, TState state)
        {
            _source = source;
            _selector = selector;
            _state = state;
        }

        protected override IDisposable SubscribeCore(Observer<TResult> observer)
        {
            return _source.Subscribe(new SelectObserver(observer, _selector, _state));
        }

        private sealed class SelectObserver : Observer<T>
        {
            private readonly Observer<TResult>        _observer;
            private readonly Func<T, TState, TResult> _selector;
            private readonly TState                   _state;

            public SelectObserver(Observer<TResult> observer, Func<T, TState, TResult> selector, TState state)
            {
                _observer = observer;
                _selector = selector;
                _state = state;
            }

            protected override void OnNextCore(T value)
            {
                _observer.OnNext(_selector(value, _state));
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                _observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                _observer.OnCompleted(result);
            }
        }
    }

    internal sealed class WhereSelect<T, TResult> : Observable<TResult>
    {
        private readonly Func<T, bool>    _predicate;
        private readonly Func<T, TResult> _selector;
        private readonly Observable<T>    _source;

        public WhereSelect(Observable<T> source, Func<T, TResult> selector, Func<T, bool> predicate)
        {
            _source = source;
            _selector = selector;
            _predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<TResult> observer)
        {
            return _source.Subscribe(new WhereSelectObserver(observer, _selector, _predicate));
        }

        private sealed class WhereSelectObserver : Observer<T>
        {
            private readonly Observer<TResult> _observer;
            private readonly Func<T, bool>     _predicate;
            private readonly Func<T, TResult>  _selector;

            public WhereSelectObserver(Observer<TResult> observer, Func<T, TResult> selector, Func<T, bool> predicate)
            {
                _observer = observer;
                _selector = selector;
                _predicate = predicate;
            }

            protected override void OnNextCore(T value)
            {
                if (_predicate(value))
                {
                    _observer.OnNext(_selector(value));
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                _observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                _observer.OnCompleted(result);
            }
        }
    }

    internal sealed class SelectIndexed<T, TResult>
        : Observable<TResult>
    {
        private readonly Func<T, int, TResult> _selector;
        private readonly Observable<T>         _source;

        public SelectIndexed(Observable<T> source, Func<T, int, TResult> selector)
        {
            _source = source;
            _selector = selector;
        }

        protected override IDisposable SubscribeCore(Observer<TResult> observer)
        {
            return _source.Subscribe(new SelectIndexedObserver(observer, _selector));
        }

        private sealed class SelectIndexedObserver : Observer<T>
        {
            private readonly Observer<TResult>     _observer;
            private readonly Func<T, int, TResult> _selector;
            private          int                   _index;

            public SelectIndexedObserver(Observer<TResult> observer, Func<T, int, TResult> selector)
            {
                _observer = observer;
                _selector = selector;
            }

            protected override void OnNextCore(T value)
            {
                _observer.OnNext(_selector(value, _index++));
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                _observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                _observer.OnCompleted(result);
            }
        }
    }

    internal sealed class SelectIndexed<T, TResult, TState> : Observable<TResult>
    {
        private readonly Func<T, int, TState, TResult> _selector;
        private readonly Observable<T>                 _source;
        private readonly TState                        _state;

        public SelectIndexed(Observable<T> source, Func<T, int, TState, TResult> selector, TState state)
        {
            _source = source;
            _selector = selector;
            _state = state;
        }

        protected override IDisposable SubscribeCore(Observer<TResult> observer)
        {
            return _source.Subscribe(new SelectIndexedObserver(observer, _selector, _state));
        }

        private sealed class SelectIndexedObserver : Observer<T>
        {
            private readonly Observer<TResult>             _observer;
            private readonly Func<T, int, TState, TResult> _selector;
            private readonly TState                        _state;
            private          int                           _index;

            public SelectIndexedObserver(Observer<TResult> observer, Func<T, int, TState, TResult> selector,
                TState state)
            {
                _observer = observer;
                _selector = selector;
                _state = state;
            }

            protected override void OnNextCore(T value)
            {
                _observer.OnNext(_selector(value, _index++, _state));
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                _observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                _observer.OnCompleted(result);
            }
        }
    }
}