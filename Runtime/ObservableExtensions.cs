// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using Edanoue.Rx.Internal;
using Edanoue.Rx.Operators;

namespace Edanoue.Rx
{
    public static partial class Observable
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
        /// Filters the elements of an observable sequence based on a predicate function.
        /// </summary>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>
        /// An observable sequence that contains elements from the input sequence that satisfy the condition specified
        /// by the predicate.
        /// </returns>
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, int, bool> predicate)
        {
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

        public static IObservable<T> SkipWhile<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return new SkipWhileObservable<T>(source, predicate);
        }

        public static IObservable<T> SkipWhile<T>(this IObservable<T> source, Func<T, int, bool> predicate)
        {
            return new SkipWhileObservable<T>(source, predicate);
        }

        /// <summary>
        /// 指定した回数分だけ OnNext を実行する, 回数が消費された瞬間に OnCompleted が呼ばれる.
        /// </summary>
        /// <remarks>
        /// 0 を指定した場合は Empty が帰る (Subscribe の時点で OnComplete が即座に呼ばれる)
        /// </remarks>
        /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <param name="count">The number of elements to take.</param>
        /// <returns>
        /// An observable sequence that contains the specified number of elements from the beginning of the source
        /// sequence.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the count is less than 0.</exception>
        public static IObservable<T> Take<T>(this IObservable<T> source, int count)
        {
            switch (count)
            {
                case < 0:
                    throw new ArgumentOutOfRangeException(nameof(count));
                case 0:
                    return Empty<T>();
            }

            // optimize .Take(count).Take(count)
            if (source is TakeObservable<T> take)
            {
                return take.Combine(count);
            }

            return new TakeObservable<T>(source, count);
        }

        /// <summary>
        /// 指定した predicate が false を返すまで OnNext を実行する. その後 OnCompleted が呼ばれる.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> TakeWhile<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return new TakeWhileObservable<T>(source, predicate);
        }

        public static IObservable<T> TakeWhile<T>(this IObservable<T> source, Func<T, int, bool> predicate)
        {
            return new TakeWhileObservable<T>(source, predicate);
        }

        /// <summary>
        /// Converting .Select(_ => Unit.Default) sequence.
        /// </summary>
        public static IObservable<Unit> AsUnitObservable<T>(this IObservable<T> source)
        {
            return new AsUnitObservableObservable<T>(source);
        }
    }
}