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
        /// <param name="duration"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Skip<T>(this Observable<T> source, TimeSpan duration)
        {
            return Skip(source, duration, ObservableSystem.DefaultTimeProvider);
        }

        /// <summary>
        /// ToDo: Unity の Time ではなくて, System の TimeProvider を使用しています
        /// </summary>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        /// <param name="timeProvider"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<T> Skip<T>(this Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
        {
            return new SkipTime<T>(source, duration.Normalize(), timeProvider);
        }
    }

    internal sealed class SkipTime<T> : Observable<T>
    {
        private readonly TimeSpan      _duration;
        private readonly Observable<T> _source;
        private readonly TimeProvider  _timeProvider;

        public SkipTime(Observable<T> source, TimeSpan duration, TimeProvider timeProvider)
        {
            _source = source;
            _duration = duration;
            _timeProvider = timeProvider;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            return _source.Subscribe(new _SkipTime(observer, _duration, _timeProvider));
        }

        private sealed class _SkipTime : Observer<T>, IDisposable
        {
            private static readonly TimerCallback _timerCallback = TimerStopped;

            private readonly Observer<T> _observer;
            private          ITimer?     _timer; // when null, the timer has been stopped

            public _SkipTime(Observer<T> observer, TimeSpan duration, TimeProvider timeProvider)
            {
                _observer = observer;
                _timer = timeProvider.CreateStoppedTimer(_timerCallback, this);
                _timer.InvokeOnce(duration);
            }

            protected override void OnNextCore(T value)
            {
                if (Volatile.Read(ref _timer) != null)
                {
                    return;
                }

                _observer.OnNext(value);
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                _observer.OnErrorResume(error);
            }

            protected override void OnCompletedCore(Result result)
            {
                _observer.OnCompleted(result);
            }

            private static void TimerStopped(object? state)
            {
                var self = (_SkipTime)state!;
                Volatile.Read(ref self._timer)?.Dispose();
                Volatile.Write(ref self._timer, null);
            }

            protected override void DisposeCore()
            {
                Volatile.Read(ref _timer)?.Dispose();
                Volatile.Write(ref _timer, null);
            }
        }
    }
}
#endif