// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx
{
    // should be use Interlocked.CompareExchange for Threadsafe?
    // but CompareExchange cause ExecutionEngineException on iOS.
    // AOT...
    // use lock instead

    public sealed class SingleAssignmentDisposable : IDisposable
    {
        private readonly object       _gate = new();
        private          IDisposable? _current;
        private          bool         _disposed;

        public bool IsDisposed
        {
            get
            {
                lock (_gate)
                {
                    return _disposed;
                }
            }
        }

        public IDisposable? Disposable
        {
            get => _current;
            set
            {
                IDisposable? old;
                bool alreadyDisposed;
                lock (_gate)
                {
                    alreadyDisposed = _disposed;
                    old = _current;
                    if (!alreadyDisposed)
                    {
                        if (value is null)
                        {
                            return;
                        }

                        _current = value;
                    }
                }

                if (alreadyDisposed && value is not null)
                {
                    value.Dispose();
                    return;
                }

                if (old is not null)
                {
                    throw new InvalidOperationException("Disposable is already set");
                }
            }
        }

        public void Dispose()
        {
            IDisposable? old = null;

            lock (_gate)
            {
                if (!_disposed)
                {
                    _disposed = true;
                    old = _current;
                    _current = null;
                }
            }

            old?.Dispose();
        }
    }
}