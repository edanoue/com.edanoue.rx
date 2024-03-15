// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        public static Observable<TSource> DistinctBy<TSource, TKey>(this Observable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return DistinctBy(source, keySelector, EqualityComparer<TKey>.Default);
        }

        public static Observable<TSource> DistinctBy<TSource, TKey>(this Observable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return new DistinctBy<TSource, TKey>(source, keySelector, comparer);
        }
    }

    internal sealed class DistinctBy<T, TKey> : Observable<T>
    {
        private readonly IEqualityComparer<TKey> _comparer;
        private readonly Func<T, TKey>           _keySelector;
        private readonly Observable<T>           _source;

        public DistinctBy(Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            _source = source;
            _keySelector = keySelector;
            _comparer = comparer;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _DistinctBy(observer, _keySelector, _comparer));
        }

        private sealed class _DistinctBy : Observer<T>
        {
            private readonly Func<T, TKey> _keySelector;
            private readonly Observer<T>   _observer;
            private readonly HashSet<TKey> _set;

            public _DistinctBy(Observer<T> observer, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
            {
                _observer = observer;
                _keySelector = keySelector;
                _set = new HashSet<TKey>(comparer);
            }

            protected override void OnNextCore(T value)
            {
                var key = _keySelector(value);
                if (_set.Add(key))
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