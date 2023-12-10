// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx
{
    public static class Disposable
    {
        public static readonly IDisposable Empty = EmptyDisposable.Singleton;

        private class EmptyDisposable : IDisposable
        {
            public static readonly EmptyDisposable Singleton = new();

            private EmptyDisposable()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}