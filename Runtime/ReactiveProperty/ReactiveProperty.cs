// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;

namespace Edanoue.Rx
{
    public abstract class ReadOnlyReactiveProperty<T> : Observable<T>, IDisposable
    {
        /// <summary>
        /// </summary>
        public abstract T CurrentValue { get; }

        public abstract void Dispose();

        public ReadOnlyReactiveProperty<T> ToReadOnlyReactiveProperty()
        {
            return this;
        }
    }

    public class ReactiveProperty<T> : ReadOnlyReactiveProperty<T>, ISubject<T>
    {
        private const byte _NOT_COMPLETED     = 0;
        private const byte _COMPLETED_SUCCESS = 1;
        private const byte _COMPLETED_FAILURE = 2;
        private const byte _DISPOSED          = 3;

        // Memory Size: 1(byte) + 8(IntPtr) + sizeof(T) + 8(IntPtr) and subscriptions(nodes).
        private byte       _completeState;
        private T          _currentValue;
        private Exception? _error;

        // For reduce memory usage, ReactiveProperty<T> itself is LinkedList and subscription represents LinkedListNode.
        // The last of node is root.Previous(if null, single list).
        private ObserverNode? _root;

        public ReactiveProperty() : this(default!)
        {
        }

        public ReactiveProperty(T value) : this(value, EqualityComparer<T>.Default)
        {
        }

        public ReactiveProperty(T value, IEqualityComparer<T>? equalityComparer)
        {
            EqualityComparer = equalityComparer;
            _currentValue = value;
        }

        public IEqualityComparer<T>? EqualityComparer { get; }

        public override T CurrentValue => _currentValue;

        public bool HasObservers => Volatile.Read(ref _root) != null;
        public bool IsCompleted => _completeState is _COMPLETED_SUCCESS or _COMPLETED_FAILURE;
        public bool IsDisposed => _completeState == _DISPOSED;
        public bool IsCompletedOrDisposed => IsCompleted || IsDisposed;


        public T Value
        {
            get => _currentValue;
            set
            {
                if (EqualityComparer != null)
                {
                    if (EqualityComparer.Equals(_currentValue, value))
                    {
                        return;
                    }
                }

                _currentValue = value;
                OnNextCore(value);
            }
        }

        public void OnNext(T value)
        {
            _currentValue = value; // different from Subject<T>; set value before raise OnNext
            OnNextCore(value);
        }

        public void OnErrorResume(Exception error)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                return;
            }

            var node = Volatile.Read(ref _root);
            var last = node?.Previous;
            while (node != null)
            {
                node.Observer.OnErrorResume(error);
                if (node == last)
                {
                    return;
                }

                node = node.Next;
            }
        }

        public void OnCompleted(Result result)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                return;
            }

            ObserverNode? node = null;
            lock (this) // I know lock(this) is dangerous.
            {
                if (_completeState == _NOT_COMPLETED)
                {
                    _completeState = result.IsSuccess ? _COMPLETED_SUCCESS : _COMPLETED_FAILURE;
                    _error = result.Exception;
                    node = Volatile.Read(ref _root);
                    Volatile.Write(ref _root, null); // when complete, List is clear.
                }
                else
                {
                    // IsCompleted = do-nothing, IsDisposed = throw
                    ThrowIfDisposed();
                    return;
                }
            }

            var last = node?.Previous;
            while (node != null)
            {
                node.Observer.OnCompleted(result);
                if (node == last)
                {
                    return;
                }

                node = node.Next;
            }
        }

        private void OnNextCore(T value)
        {
            ThrowIfDisposed();

            if (IsCompleted)
            {
                return;
            }

            var node = Volatile.Read(ref _root);
            var last = node?.Previous;
            while (node != null)
            {
                node.Observer.OnNext(value);
                if (node == last)
                {
                    return;
                }

                node = node.Next;
            }
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("");
            }
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            Result? completedResult;
            lock (this)
            {
                ThrowIfDisposed();
                if (IsCompleted)
                {
                    completedResult = _error == null ? Result.Success : Result.Failure(_error);
                }
                else
                {
                    completedResult = null;
                }
            }

            if (completedResult != null)
            {
                goto PUBLISH_CURRENT_AND_RESULT;
            }

            // raise latest value on subscribe(before add observer to list)
            observer.OnNext(_currentValue);

            lock (this)
            {
                ThrowIfDisposed();
                if (IsCompleted)
                {
                    completedResult = _error == null ? Result.Success : Result.Failure(_error);
                    goto PUBLISH_RESULT;
                }

                // create subscription and add to list in lock.
                var subscription = new ObserverNode(this, observer);
                return subscription;
            }

            PUBLISH_CURRENT_AND_RESULT:
            if (completedResult.Value.IsSuccess)
            {
                observer.OnNext(_currentValue);
            }

            observer.OnCompleted(completedResult.Value);
            return Disposable.Empty;

            PUBLISH_RESULT:
            observer.OnCompleted(completedResult.Value);
            return Disposable.Empty;
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool callOnCompleted)
        {
            ObserverNode? node = null;
            lock (this)
            {
                if (_completeState == _DISPOSED)
                {
                    return;
                }

                // not yet disposed so can call list iteration
                if (callOnCompleted && !IsCompleted)
                {
                    node = Volatile.Read(ref _root);
                }

                Volatile.Write(ref _root, null);
                _completeState = _DISPOSED;
            }

            while (node != null)
            {
                node.Observer.OnCompleted();
                node = node.Next;
            }
        }


        public override string? ToString()
        {
            return _currentValue == null ? "(null)" : _currentValue.ToString();
        }

        private sealed class ObserverNode : IDisposable
        {
            public readonly Observer<T> Observer;

            private ReactiveProperty<T>? _parent;

            public ObserverNode(ReactiveProperty<T> parent, Observer<T> observer)
            {
                _parent = parent;
                Observer = observer;

                if (parent._root == null)
                {
                    // Single list(both previous and next is null)
                    Volatile.Write(ref parent._root, this);
                }
                else
                {
                    // previous is last, null then root is last.
                    var lastNode = parent._root.Previous ?? parent._root;

                    lastNode.Next = this;
                    Previous = lastNode;
                    parent._root.Previous = this;
                }
            }

            public ObserverNode? Previous { get; private set; } // Previous is last node or root(null).
            public ObserverNode? Next { get; private set; }

            public void Dispose()
            {
                var p = Interlocked.Exchange(ref _parent, null);
                if (p == null)
                {
                    return;
                }

                // keep this.Next for dispose on iterating
                // Remove node(self) from list(ReactiveProperty)
                lock (p)
                {
                    if (p.IsCompletedOrDisposed)
                    {
                        return;
                    }

                    if (this == p._root)
                    {
                        if (Previous == null || Next == null)
                        {
                            // case of single list
                            p._root = null;
                        }
                        else
                        {
                            // otherwise, root is next node.
                            var root = Next;

                            // single list.
                            if (root.Next == null)
                            {
                                root.Previous = null;
                            }
                            else
                            {
                                root.Previous = Previous; // as last.
                            }

                            p._root = root;
                        }
                    }
                    else
                    {
                        // node is not root, previous must exists
                        Previous!.Next = Next;
                        if (Next != null)
                        {
                            Next.Previous = Previous;
                        }
                        else
                        {
                            // next does not exists, previous is last node so modify root
                            p._root!.Previous = Previous;
                        }
                    }
                }
            }
        }
    }
}