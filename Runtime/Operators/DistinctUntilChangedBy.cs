// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        public static Observable<T> DistinctUntilChangedBy<T, TKey>(this Observable<T> source,
            Func<T, TKey> keySelector)
        {
            return DistinctUntilChangedBy(source, keySelector, EqualityComparer<TKey>.Default);
        }

        public static Observable<T> DistinctUntilChangedBy<T, TKey>(this Observable<T> source,
            Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return new DistinctUntilChangedBy<T, TKey>(source, keySelector, comparer);
        }
    }

    internal sealed class DistinctUntilChangedBy<T, TKey> : Observable<T>
    {
        private readonly IEqualityComparer<TKey> _comparer;
        private readonly Func<T, TKey>           _keySelector;
        private readonly Observable<T>           _source;

        public DistinctUntilChangedBy(Observable<T> source, Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            _source = source;
            _keySelector = keySelector;
            _comparer = comparer;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _DistinctUntilChangedBy(observer, _keySelector, _comparer));
        }

        private sealed class _DistinctUntilChangedBy : Observer<T>
        {
            private readonly IEqualityComparer<TKey> _comparer;
            private readonly Func<T, TKey>           _keySelector;
            private readonly Observer<T>             _observer;
            private          bool                    _hasValue;
            private          TKey?                   _lastKey;

            public _DistinctUntilChangedBy(Observer<T> observer, Func<T, TKey> keySelector,
                IEqualityComparer<TKey> comparer)
            {
                _observer = observer;
                _keySelector = keySelector;
                _comparer = comparer;
            }

            protected override void OnNextCore(T value)
            {
                var key = _keySelector(value);
                if (!_hasValue || !_comparer.Equals(_lastKey!, key))
                {
                    _hasValue = true;
                    _lastKey = key;
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