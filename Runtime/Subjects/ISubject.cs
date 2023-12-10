// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx
{
    public interface ISubject<in TSource, out TResult> : IObserver<TSource>, IObservable<TResult>
    {
    }

    public interface ISubject<T> : ISubject<T, T>, IObserver<T>, IObservable<T>
    {
    }
}