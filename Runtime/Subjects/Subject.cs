// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using Edanoue.Rx.Internal;

namespace Edanoue.Rx
{
    public sealed class Subject<T> : ISubject<T>, IDisposable
    {
        private readonly object _observerLock = new();
        private          bool   _isDisposed;

        private bool         _isStopped;
        private Exception?   _lastError;
        private IObserver<T> _outObserver = NoOpObserver<T>.Default;

        public void Dispose()
        {
            lock (_observerLock)
            {
                _isDisposed = true;
                _outObserver = DisposedObserver<T>.Default;
            }
        }

        public void OnCompleted()
        {
            IObserver<T> old;
            lock (_observerLock)
            {
                ThrowIfDisposed();
                if (_isStopped)
                {
                    return;
                }

                old = _outObserver;
                _outObserver = NoOpObserver<T>.Default;
                _isStopped = true;
            }

            old.OnCompleted();
        }

        public void OnError(Exception error)
        {
            IObserver<T> old;
            lock (_observerLock)
            {
                ThrowIfDisposed();
                if (_isStopped)
                {
                    return;
                }

                old = _outObserver;
                _outObserver = NoOpObserver<T>.Default;
                _isStopped = true;
                _lastError = error;
            }

            old.OnError(error);
        }

        public void OnNext(T value)
        {
            _outObserver.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            Exception? ex;

            lock (_observerLock)
            {
                ThrowIfDisposed();

                // まだ Completed ではないばあい
                if (!_isStopped)
                {
                    if (_outObserver is ListObserver<T> listObserver)
                    {
                        _outObserver = listObserver.Add(observer);
                    }
                    else
                    {
                        var current = _outObserver;
                        if (current is NoOpObserver<T>)
                        {
                            _outObserver = observer;
                        }
                        else
                        {
                            _outObserver =
                                new ListObserver<T>(new ImmutableList<IObserver<T>>(new[] { current, observer }));
                        }
                    }

                    return new Subscription(this, observer);
                }

                ex = _lastError;
            }

            if (ex is not null)
            {
                observer.OnError(ex);
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Empty;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("");
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly object        _gate = new();
            private          Subject<T>?   _parent;
            private          IObserver<T>? _unsubscribeTarget;

            public Subscription(Subject<T> parent, IObserver<T> unsubscribeTarget)
            {
                _parent = parent;
                _unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                lock (_gate)
                {
                    if (_parent is null)
                    {
                        return;
                    }

                    lock (_parent._observerLock)
                    {
                        if (_parent._outObserver is ListObserver<T> listObserver)
                        {
                            _parent._outObserver = listObserver.Remove(_unsubscribeTarget);
                        }
                        else
                        {
                            _parent._outObserver = NoOpObserver<T>.Default;
                        }

                        _unsubscribeTarget = null;
                        _parent = null;
                    }
                }
            }
        }
    }
}