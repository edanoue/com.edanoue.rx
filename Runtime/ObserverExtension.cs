// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx
{
    public static class ObserverExtensions
    {
        public static void OnCompleted<T>(this Observer<T> observer)
        {
            observer.OnCompleted(Result.Success);
        }

        public static void OnCompleted<T>(this Observer<T> observer, Exception exception)
        {
            observer.OnCompleted(Result.Failure(exception));
        }
    }
}