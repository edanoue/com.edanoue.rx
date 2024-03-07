// Copyright Edanoue, Inc. All Rights Reserved.

using System;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> SkipWhile<T>(this Observable<T> source, Func<T, bool> predicate)
        {
            return new SkipWhile<T>(source, predicate);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> SkipWhile<T>(this Observable<T> source, Func<T, int, bool> predicate)
        {
            return new SkipWhileI<T>(source, predicate);
        }
    }

    internal sealed class SkipWhile<T> : Observable<T>
    {
        private readonly Func<T, bool> _predicate;
        private readonly Observable<T> _source;

        public SkipWhile(Observable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new SkipWhileObserver(observer, _predicate));
        }

        private sealed class SkipWhileObserver : Observer<T>
        {
            private readonly Observer<T>   _observer;
            private readonly Func<T, bool> _predicate;

            private bool _open;

            public SkipWhileObserver(Observer<T> observer, Func<T, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            protected override void OnNextCore(T value)
            {
                if (_open)
                {
                    _observer.OnNext(value);
                }
                else if (!_predicate(value))
                {
                    _open = true;
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

    internal sealed class SkipWhileI<T> : Observable<T>
    {
        private readonly Func<T, int, bool> _predicate;
        private readonly Observable<T>      _source;

        public SkipWhileI(Observable<T> source, Func<T, int, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new SkipWhileIObserver(observer, _predicate));
        }

        private sealed class SkipWhileIObserver : Observer<T>
        {
            private readonly Observer<T>        _observer;
            private readonly Func<T, int, bool> _predicate;
            private          int                _count;
            private          bool               _open;

            public SkipWhileIObserver(Observer<T> observer, Func<T, int, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            protected override void OnNextCore(T value)
            {
                if (_open)
                {
                    _observer.OnNext(value);
                }
                else if (!_predicate(value, _count++))
                {
                    _open = true;
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