// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        public static Observable<T> Distinct<T>(this Observable<T> source)
        {
            return Distinct(source, EqualityComparer<T>.Default);
        }

        public static Observable<T> Distinct<T>(this Observable<T> source, IEqualityComparer<T> comparer)
        {
            return new Distinct<T>(source, comparer);
        }
    }

    internal sealed class Distinct<T> : Observable<T>
    {
        private readonly IEqualityComparer<T> _comparer;
        private readonly Observable<T>        _source;

        public Distinct(Observable<T> source, IEqualityComparer<T> comparer)
        {
            _source = source;
            _comparer = comparer;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _Distinct(observer, _comparer));
        }

        private sealed class _Distinct : Observer<T>
        {
            private readonly Observer<T> _observer;
            private readonly HashSet<T>  _set;

            public _Distinct(Observer<T> observer, IEqualityComparer<T> comparer)
            {
                _observer = observer;
                _set = new HashSet<T>(comparer);
            }

            protected override void OnNextCore(T value)
            {
                if (_set.Add(value))
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