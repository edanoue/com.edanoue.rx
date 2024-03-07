// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// Filters the values emitted by the source observable sequence based on the specified predicate function.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source observable sequence.</typeparam>
        /// <param name="source">The source observable sequence to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An observable sequence that contains elements from the input sequence that satisfy the condition.</returns>
        public static Observable<T> Where<T>(this Observable<T> source, Func<T, bool> predicate)
        {
            if (source is not Where<T> where)
            {
                return new Where<T>(source, predicate);
            }

            // Optimize for Where.Where, create combined predicate.
            var p = where.Predicate;
            // Note: lambda captured but don't use TState to allow combine more Where
            return new Where<T>(where.Source, x => p(x) && predicate(x));
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Where<T>(this Observable<T> source, Func<T, int, bool> predicate)
        {
            return new WhereIndexed<T>(source, predicate);
        }


        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="state"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public static Observable<T> Where<T, TState>(this Observable<T> source, TState state,
            Func<T, TState, bool> predicate)
        {
            return new Where<T, TState>(source, predicate, state);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="state"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <returns></returns>
        public static Observable<T> Where<T, TState>(this Observable<T> source, TState state,
            Func<T, int, TState, bool> predicate)
        {
            return new WhereIndexed<T, TState>(source, predicate, state);
        }
    }

    internal sealed class Where<T> : Observable<T>
    {
        public readonly Func<T, bool> Predicate;
        public readonly Observable<T> Source;

        public Where(Observable<T> source, Func<T, bool> predicate)
        {
            Source = source;
            Predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return Source.Subscribe(new WhereInternal(observer, Predicate));
        }

        private class WhereInternal : Observer<T>
        {
            private readonly Observer<T>   _observer;
            private readonly Func<T, bool> _predicate;

            public WhereInternal(Observer<T> observer, Func<T, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            protected override void OnNextCore(T value)
            {
                if (_predicate(value))
                {
                    _observer.OnNext(value);
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

    internal sealed class WhereIndexed<T> : Observable<T>
    {
        private readonly Func<T, int, bool> _predicate;
        private readonly Observable<T>      _source;

        public WhereIndexed(Observable<T> source, Func<T, int, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new WhereIndexedInternal(observer, _predicate));
        }

        private class WhereIndexedInternal : Observer<T>
        {
            private readonly Observer<T>        _observer;
            private readonly Func<T, int, bool> _predicate;

            private int _index;

            public WhereIndexedInternal(Observer<T> observer, Func<T, int, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            protected override void OnNextCore(T value)
            {
                if (_predicate(value, _index++))
                {
                    _observer.OnNext(value);
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

    internal sealed class Where<T, TState> : Observable<T>
    {
        private readonly Func<T, TState, bool> _predicate;
        private readonly Observable<T>         _source;
        private readonly TState                _state;

        public Where(Observable<T> source, Func<T, TState, bool> predicate, TState state)
        {
            _source = source;
            _predicate = predicate;
            _state = state;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new WhereInternal(observer, _predicate, _state));
        }

        private class WhereInternal : Observer<T>
        {
            private readonly Observer<T>           _observer;
            private readonly Func<T, TState, bool> _predicate;
            private readonly TState                _state;

            public WhereInternal(Observer<T> observer, Func<T, TState, bool> predicate, TState state)
            {
                _observer = observer;
                _predicate = predicate;
                _state = state;
            }

            protected override void OnNextCore(T value)
            {
                if (_predicate(value, _state))
                {
                    _observer.OnNext(value);
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

    internal sealed class WhereIndexed<T, TState> : Observable<T>
    {
        private readonly Func<T, int, TState, bool> _predicate;
        private readonly Observable<T>              _source;
        private readonly TState                     _state;

        public WhereIndexed(Observable<T> source, Func<T, int, TState, bool> predicate, TState state)
        {
            _source = source;
            _predicate = predicate;
            _state = state;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new WhereIndexedInternal(observer, _predicate, _state));
        }

        private class WhereIndexedInternal : Observer<T>
        {
            private readonly Observer<T>                _observer;
            private readonly Func<T, int, TState, bool> _predicate;
            private readonly TState                     _state;
            private          int                        _index;

            public WhereIndexedInternal(Observer<T> observer, Func<T, int, TState, bool> predicate, TState state)
            {
                _observer = observer;
                _predicate = predicate;
                _state = state;
            }

            protected override void OnNextCore(T value)
            {
                if (_predicate(value, _index++, _state))
                {
                    _observer.OnNext(value);
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
}