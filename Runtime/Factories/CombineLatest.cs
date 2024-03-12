// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Edanoue.Rx
{
    public static partial class Observable
    {
        /// <summary>
        /// </summary>
        /// <param name="sources"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T[]> CombineLatest<T>(params Observable<T>[] sources)
        {
            return new CombineLatest<T>(sources);
        }

        /// <summary>
        /// </summary>
        /// <param name="sources"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T[]> CombineLatest<T>(IEnumerable<Observable<T>> sources)
        {
            return new CombineLatest<T>(sources);
        }
    }

    internal sealed class CombineLatest<T> : Observable<T[]>
    {
        private readonly IEnumerable<Observable<T>> _sources;

        public CombineLatest(IEnumerable<Observable<T>> sources)
        {
            _sources = sources;
        }

        protected override IDisposable SubscribeCore(Observer<T[]> observer)
        {
            return new _CombineLatest(observer, _sources).Run();
        }

        private sealed class _CombineLatest : IDisposable
        {
            private readonly Observer<T[]>           _observer;
            private readonly CombineLatestObserver[] _observers;
            private readonly Observable<T>[]         _sources;
            private          int                     _completedCount;
            private          bool                    _hasValueAll;

            public _CombineLatest(Observer<T[]> observer, IEnumerable<Observable<T>> sources)
            {
                _observer = observer;
                if (sources is Observable<T>[] array)
                {
                    _sources = array;
                }
                else
                {
                    _sources = sources.ToArray();
                }

                var observers = new CombineLatestObserver[_sources.Length];
                for (var i = 0; i < observers.Length; i++)
                {
                    observers[i] = new CombineLatestObserver(this);
                }

                _observers = observers;
            }

            public void Dispose()
            {
                foreach (var observer in _observers)
                {
                    observer.Dispose();
                }
            }

            public IDisposable Run()
            {
                try
                {
                    for (var i = 0; i < _sources.Length; i++)
                    {
                        _sources[i].Subscribe(_observers[i]);
                    }
                }
                catch
                {
                    Dispose();
                    throw;
                }

                return this;
            }

            public void TryPublishOnNext()
            {
                if (!_hasValueAll)
                {
                    foreach (var item in _observers)
                    {
                        if (!item.HasValue)
                        {
                            return;
                        }
                    }

                    _hasValueAll = true;
                }

                var values = new T[_observers.Length];
                for (var i = 0; i < _observers.Length; i++)
                {
                    values[i] = _observers[i].Value!;
                }

                _observer.OnNext(values);
            }

            public void TryPublishOnCompleted(Result result)
            {
                if (result.IsFailure)
                {
                    _observer.OnCompleted(result);
                    Dispose();
                }
                else
                {
                    if (Interlocked.Increment(ref _completedCount) == _sources.Length)
                    {
                        _observer.OnCompleted();
                        Dispose();
                    }
                }
            }

            private sealed class CombineLatestObserver : Observer<T>
            {
                private readonly _CombineLatest _parent;

                public CombineLatestObserver(_CombineLatest parent)
                {
                    _parent = parent;
                }

                public T? Value { get; private set; }

                // [MemberNotNullWhen(true, nameof(Value))]
                public bool HasValue { get; private set; }

                protected override void OnNextCore(T value)
                {
                    lock (_parent._observers)
                    {
                        Value = value;
                        HasValue = true;
                        _parent.TryPublishOnNext();
                    }
                }

                protected override void OnErrorResumeCore(Exception error)
                {
                    _parent._observer.OnErrorResume(error);
                }

                protected override void OnCompletedCore(Result result)
                {
                    _parent.TryPublishOnCompleted(result);
                }
            }
        }
    }
}