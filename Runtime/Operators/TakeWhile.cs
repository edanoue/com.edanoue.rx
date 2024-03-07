// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

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
        public static Observable<T> TakeWhile<T>(this Observable<T> source, Func<T, bool> predicate)
        {
            return new TakeWhile<T>(source, predicate);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> TakeWhile<T>(this Observable<T> source, Func<T, int, bool> predicate)
        {
            return new TakeWhileI<T>(source, predicate);
        }
    }

    internal sealed class TakeWhile<T> : Observable<T>
    {
        private readonly Func<T, bool> _predicate;
        private readonly Observable<T> _source;

        public TakeWhile(Observable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new TakeWhileInternal(observer, _predicate));
        }

        private sealed class TakeWhileInternal : Observer<T>
        {
            private readonly Observer<T>   _observer;
            private readonly Func<T, bool> _predicate;

            public TakeWhileInternal(Observer<T> observer, Func<T, bool> predicate)
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
                else
                {
                    _observer.OnCompleted();
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

    internal sealed class TakeWhileI<T> : Observable<T>
    {
        private readonly Func<T, int, bool> _predicate;
        private readonly Observable<T>      _source;

        public TakeWhileI(Observable<T> source, Func<T, int, bool> predicate)
        {
            _source = source;
            _predicate = predicate;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new TakeWhileIInternal(observer, _predicate));
        }

        private sealed class TakeWhileIInternal : Observer<T>
        {
            private readonly Observer<T>        _observer;
            private readonly Func<T, int, bool> _predicate;
            private          int                _count;

            public TakeWhileIInternal(Observer<T> observer, Func<T, int, bool> predicate)
            {
                _observer = observer;
                _predicate = predicate;
            }

            protected override void OnNextCore(T value)
            {
                if (_predicate(value, _count++))
                {
                    _observer.OnNext(value);
                }
                else
                {
                    _observer.OnCompleted();
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