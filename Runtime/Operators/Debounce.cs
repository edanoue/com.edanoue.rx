// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
#if SUPPORT_TIME_PROVIDER
using System;
using System.Threading;
using Edanoue.Rx.Internal;

namespace Edanoue.Rx
{
    public static partial class ObservableExtensions
    {
        /// <summary>
        /// ToDo: Unity の Time ではなくて, System の TimeProvider を使用しています
        /// </summary>
        /// <param name="source"></param>
        /// <param name="timeSpan"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Debounce<T>(this Observable<T> source, TimeSpan timeSpan)
        {
            return new Debounce<T>(source, timeSpan, ObservableSystem.DefaultTimeProvider);
        }

        /// <summary>
        /// ToDo: Unity の Time ではなくて, System の TimeProvider を使用しています
        /// </summary>
        /// <param name="source"></param>
        /// <param name="timeSpan"></param>
        /// <param name="timeProvider"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Debounce<T>(this Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider)
        {
            return new Debounce<T>(source, timeSpan, timeProvider);
        }
    }

    internal sealed class Debounce<T> : Observable<T>
    {
        private readonly Observable<T> _source;
        private readonly TimeProvider  _timeProvider;
        private readonly TimeSpan      _timeSpan;

        public Debounce(Observable<T> source, TimeSpan timeSpan, TimeProvider timeProvider)
        {
            _source = source;
            _timeSpan = timeSpan;
            _timeProvider = timeProvider;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _Debounce(observer, _timeSpan.Normalize(), _timeProvider));
        }

        private sealed class _Debounce : Observer<T>
        {
            private static readonly TimerCallback _timerCallback = RaiseOnNext;
            private readonly        object        _gate          = new();

            private readonly Observer<T> _observer;
            private readonly ITimer      _timer;
            private readonly TimeSpan    _timeSpan;
            private          bool        _hasvalue;
            private          T?          _latestValue;
            private          int         _timerId;

            public _Debounce(Observer<T> observer, TimeSpan timeSpan, TimeProvider timeProvider)
            {
                _observer = observer;
                _timeSpan = timeSpan;
                _timer = timeProvider.CreateStoppedTimer(_timerCallback, this);
            }

            protected override void OnNextCore(T value)
            {
                lock (_gate)
                {
                    _latestValue = value;
                    _hasvalue = true;
                    Volatile.Write(ref _timerId, unchecked(_timerId + 1));
                    _timer.InvokeOnce(_timeSpan); // restart timer
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                lock (_gate)
                {
                    _observer.OnErrorResume(error);
                }
            }

            protected override void OnCompletedCore(Result result)
            {
                lock (_gate)
                {
                    if (_hasvalue)
                    {
                        _observer.OnNext(_latestValue!);
                        _hasvalue = false;
                        _latestValue = default;
                    }

                    _observer.OnCompleted(result);
                }
            }

            protected override void DisposeCore()
            {
                _timer.Dispose();
            }

            private static void RaiseOnNext(object? state)
            {
                var self = (_Debounce)state!;

                var timerId = Volatile.Read(ref self._timerId);
                lock (self._gate)
                {
                    if (timerId != self._timerId)
                    {
                        return;
                    }

                    if (!self._hasvalue)
                    {
                        return;
                    }

                    self._observer.OnNext(self._latestValue!);
                    self._hasvalue = false;
                    self._latestValue = default;
                }
            }
        }
    }
}
#endif