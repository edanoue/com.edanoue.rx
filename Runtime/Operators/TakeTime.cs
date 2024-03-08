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
        /// </summary>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Take<T>(this Observable<T> source, TimeSpan duration)
        {
            return Take(source, duration, ObservableSystem.DefaultTimeProvider);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        /// <param name="timeProvider"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Take<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
        {
            return new TakeTime<T>(source, duration.Normalize(), timeProvider);
        }
    }

    internal sealed class TakeTime<T> : Observable<T>
    {
        private readonly TimeSpan      _duration;
        private readonly Observable<T> _source;
        private readonly TimeProvider  _timeProvider;

        public TakeTime(Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
        {
            _source = source;
            _duration = duration;
            _timeProvider = timeProvider;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _TakeTime(observer, _duration, _timeProvider));
        }

        private sealed class _TakeTime : Observer<T>, IDisposable
        {
            private static readonly TimerCallback _timerCallback = TimerStopped;
            private readonly        object        _gate          = new();

            private readonly Observer<T> _observer;
            private readonly ITimer      _timer;

            public _TakeTime(Observer<T> observer, TimeSpan duration, TimeProvider timeProvider)
            {
                _observer = observer;
                _timer = timeProvider.CreateStoppedTimer(_timerCallback, this);
                _timer.InvokeOnce(duration);
            }

            protected override void OnNextCore(T value)
            {
                lock (_gate)
                {
                    _observer.OnNext(value);
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
                    _observer.OnCompleted(result);
                }
            }

            private static void TimerStopped(object? state)
            {
                var self = (_TakeTime)state!;
                self.OnCompleted();
            }

            protected override void DisposeCore()
            {
                _timer.Dispose();
            }
        }
    }
}
#endif