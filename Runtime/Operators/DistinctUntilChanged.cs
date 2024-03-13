// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        public static Observable<T> DistinctUntilChanged<T>(this Observable<T> source)
        {
            return DistinctUntilChanged(source, EqualityComparer<T>.Default);
        }

        public static Observable<T> DistinctUntilChanged<T>(this Observable<T> source, IEqualityComparer<T> comparer)
        {
            return new DistinctUntilChanged<T>(source, comparer);
        }
    }

    internal sealed class DistinctUntilChanged<T> : Observable<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly Observable<T>        _source;

        public DistinctUntilChanged(Observable<T> source, IEqualityComparer<T> comparer)
        {
            _source = source;
            _comparer = comparer;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _DistinctUntilChanged(observer, _comparer));
        }

        private sealed class _DistinctUntilChanged : Observer<T>
        {
            private readonly IEqualityComparer<T> _comparer;
            private readonly Observer<T>          _observer;
            private          bool                 _hasValue;
            private          T?                   _lastValue;

            public _DistinctUntilChanged(Observer<T> observer, IEqualityComparer<T> comparer)
            {
                _observer = observer;
                _comparer = comparer;
            }

            protected override void OnNextCore(T value)
            {
                if (!_hasValue || !_comparer.Equals(_lastValue!, value))
                {
                    _hasValue = true;
                    _lastValue = value;
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