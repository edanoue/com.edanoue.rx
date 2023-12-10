// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using Edanoue.Rx.Internal;
using Edanoue.Rx.Operators;

namespace Edanoue.Rx
{
    public static class ObservableExtensions
    {
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, Stubs.Throw, Stubs.NoOp));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, onError, Stubs.NoOp));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, Stubs.Throw, onCompleted));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError,
            Action onCompleted)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, onError, onCompleted));
        }

        /// <summary>
        /// OnNext の内容を条件式でフィルタリング.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate">条件式</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            // Optimized for Where().Where()
            if (source is WhereObservable<T> prevWhere)
            {
                return prevWhere.CombinePredicate(predicate);
            }

            /*
            var selectObservable = source as UniRx.Operators.ISelect<T>;
            if (selectObservable != null)
            {
                return selectObservable.CombinePredicate(predicate);
            }
            */

            return new WhereObservable<T>(source, predicate);
        }

        /// <summary>
        /// 指定した個数の OnNext を無視する.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count">無視する個数 (1 以上)</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IObservable<T> Skip<T>(this IObservable<T> source, int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            // optimize .Skip(count).Skip(count)
            if (source is SkipObservable<T> skip)
            {
                return skip.Combine(count);
            }

            return new SkipObservable<T>(source, count);
        }
    }
}