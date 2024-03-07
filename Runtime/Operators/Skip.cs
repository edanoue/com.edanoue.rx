// Copyright Edanoue, Inc. All Rights Reserved.

using System;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Observable<T> Skip<T>(this Observable<T> source, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            return new Skip<T>(source, count);
        }
    }

    internal sealed class Skip<T> : Observable<T>
    {
        private readonly int           _count;
        private readonly Observable<T> _source;

        public Skip(Observable<T> source, int count)
        {
            _source = source;
            _count = count;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new SkipObserver(observer, _count));
        }

        private sealed class SkipObserver : Observer<T>
        {
            private readonly Observer<T> _observer;
            private          int         _remaining;

            public SkipObserver(Observer<T> observer, int count)
            {
                _observer = observer;
                _remaining = count;
            }

            protected override void OnNextCore(T value)
            {
                if (_remaining > 0)
                {
                    _remaining--;
                }
                else
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