// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public static class DisposableExtensions
    {
        /// <summary>
        /// Add disposable(self) to CompositeDisposable(or other ICollection).
        /// Return value is self disposable.
        /// </summary>
        public static T AddTo<T>(this T disposable, ICollection<IDisposable> container)
            where T : IDisposable
        {
            container.Add(disposable);
            return disposable;
        }
    }
}