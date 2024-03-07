// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// 指定した回数分だけ OnNext を実行する, 回数が消費された瞬間に OnCompleted が呼ばれる.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Observable<T> Take<T>(this Observable<T> source, int count)
        {
            return count switch
            {
                < 0 => throw new ArgumentOutOfRangeException(nameof(count)),
                0 => Observable.Empty<T>(),
                _ => new Take<T>(source, count)
            };
        }
    }

    internal sealed class Take<T> : Observable<T>
    {
        private readonly int           _count;
        private readonly Observable<T> _source;

        public Take(Observable<T> source, int count)
        {
            _source = source;
            _count = count;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new TakeInternal(observer, _count));
        }

        private sealed class TakeInternal : Observer<T>
        {
            private readonly Observer<T> _observer;
            private          int         _remaining;

            public TakeInternal(Observer<T> observer, int count)
            {
                _observer = observer;
                _remaining = count;
            }

            protected override void OnNextCore(T value)
            {
                if (_remaining > 0)
                {
                    _remaining--;
                    _observer.OnNext(value);
                    if (_remaining == 0)
                    {
                        _observer.OnCompleted();
                    }
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