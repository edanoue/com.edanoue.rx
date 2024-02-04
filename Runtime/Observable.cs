// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;
using Edanoue.Rx.Operators;

namespace Edanoue.Rx
{
    public static partial class Observable
    {
        /// <summary>
        /// 引数の IObservable を並列に合成. 引数の OnNext いずれかが呼ばれた場合は OnNext を実行, すべての OnComplete が呼ばれたら OnComplete を実行.
        /// </summary>
        /// <param name="sources"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <returns></returns>
        public static IObservable<TSource> Merge<TSource>(params IObservable<TSource>[] sources)
        {
            return new MergeObservable<TSource>(sources.ToObservable());
        }

        private static IObservable<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return new ToObservableObservable<T>(source);
        }

        /// <summary>
        /// Empty Observable. Returns only OnCompleted.
        /// </summary>
        private static IObservable<T> Empty<T>()
        {
            return ImmutableEmptyObservable<T>.Instance;
        }
    }
}