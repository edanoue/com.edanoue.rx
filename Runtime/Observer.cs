// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using Edanoue.Rx.Internal;

namespace Edanoue.Rx
{
    public static class Observer
    {
        internal static IObserver<T> CreateSubscribeObserver<T>(Action<T> onNext, Action<Exception> onError,
            Action onCompleted)
        {
            return new Subscribe<T>(onNext, onError, onCompleted);
        }
    }
}