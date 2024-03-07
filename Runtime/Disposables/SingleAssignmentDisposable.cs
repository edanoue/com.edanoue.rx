// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;

namespace Edanoue.Rx
{
    public sealed class SingleAssignmentDisposable : IDisposable
    {
        private SingleAssignmentDisposableCore _core;

        public bool IsDisposed => _core.IsDisposed;

        public IDisposable? Disposable
        {
            get => _core.Disposable;
            set => _core.Disposable = value;
        }

        public void Dispose()
        {
            _core.Dispose();
        }
    }

    // struct, be careful to use
    public struct SingleAssignmentDisposableCore
    {
        private IDisposable? _current;

        public bool IsDisposed => Volatile.Read(ref _current) == DisposedSentinel.Instance;

        public IDisposable? Disposable
        {
            get
            {
                var field = Volatile.Read(ref _current);
                if (field == DisposedSentinel.Instance)
                {
                    return Rx.Disposable.Empty; // don't expose sentinel
                }

                return field;
            }
            set
            {
                var field = Interlocked.CompareExchange(ref _current, value, null);
                if (field == null)
                {
                    // ok to set.
                    return;
                }

                if (field == DisposedSentinel.Instance)
                {
                    // We've already been disposed, so dispose the value we've just been given.
                    value?.Dispose();
                    return;
                }

                // otherwise, invalid assignment
                ThrowAlreadyAssignment();
            }
        }

        public void Dispose()
        {
            var field = Interlocked.Exchange(ref _current, DisposedSentinel.Instance);
            if (field != DisposedSentinel.Instance)
            {
                field?.Dispose();
            }
        }

        private static void ThrowAlreadyAssignment()
        {
            throw new InvalidOperationException("Disposable is already assigned.");
        }

        private sealed class DisposedSentinel : IDisposable
        {
            public static readonly DisposedSentinel Instance = new();

            private DisposedSentinel()
            {
            }

            public void Dispose()
            {
            }
        }
    }
}