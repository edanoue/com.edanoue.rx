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
        private CompleteState _completeState; // struct(int, IntPtr)
        private T             _currentValue;
        private Subscription? _root; // Root of LinkedList Node(Root.Previous is Last)

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

        public bool IsDisposed => _completeState.IsDisposed;

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
            if (_completeState.IsCompleted)
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
            var status = _completeState.TrySetResult(result);
            if (status != CompleteState.ResultStatus.Done)
            {
                return; // already completed
            }

            var node = Volatile.Read(ref _root);
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

        public void ForceNotify()
        {
            OnNext(Value);
        }

        private void OnNextCore(T value)
        {
            if (_completeState.IsCompleted)
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

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            var result = _completeState.TryGetResult();
            if (result != null)
            {
                observer.OnNext(_currentValue);
                observer.OnCompleted(result.Value);
                return Disposable.Empty;
            }

            // raise latest value on subscribe(before add observer to list)
            observer.OnNext(_currentValue);

            var subscription = new Subscription(this, observer); // create subscription

            // need to check called completed during adding
            result = _completeState.TryGetResult();
            if (result != null)
            {
                subscription.Observer.OnCompleted(result.Value);
                subscription.Dispose();
                return Disposable.Empty;
            }

            return subscription;
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool callOnCompleted)
        {
            if (_completeState.TrySetDisposed(out var alreadyCompleted))
            {
                if (callOnCompleted && !alreadyCompleted)
                {
                    // not yet disposed so can call list iteration
                    var node = Volatile.Read(ref _root);
                    while (node != null)
                    {
                        node.Observer.OnCompleted();
                        node = node.Next;
                    }
                }

                DisposeCore();
                Volatile.Write(ref _root, null);
            }
        }

        protected virtual void DisposeCore()
        {
        }

        public override string? ToString()
        {
            return _currentValue == null ? "(null)" : _currentValue.ToString();
        }

        private sealed class Subscription : IDisposable
        {
            public readonly Observer<T> Observer;

            private ReactiveProperty<T>? _parent;

            public Subscription(ReactiveProperty<T> parent, Observer<T> observer)
            {
                _parent = parent;
                Observer = observer;

                if (parent._root == null)
                {
                    Volatile.Write(ref parent._root, this);
                    Previous = this;
                }
                else
                {
                    var last = parent._root.Previous;
                    last.Next = this;
                    Previous = last;
                    parent._root.Previous = this; // this as last
                }
            }

            public Subscription Previous { get; private set; }
            public Subscription? Next { get; private set; }

            public void Dispose()
            {
                var p = Interlocked.Exchange(ref _parent, null);
                if (p == null)
                {
                    return;
                }

                // keep this.Next for dispose on iterating

                if (Previous == this) // single list
                {
                    p._root = null;
                    return;
                }

                if (p._root == this)
                {
                    var next = Next;
                    p._root = next;
                    if (next != null)
                    {
                        if (next.Next != null)
                        {
                            next.Previous = Previous;
                        }
                        else
                        {
                            next.Previous = next;
                        }
                    }
                }
                else
                {
                    var prev = Previous;
                    var next = Next;
                    prev.Next = next;
                    if (next != null)
                    {
                        if (next.Next != null)
                        {
                            next.Previous = prev;
                        }
                    }

                    // modify root
                    if (p._root != null)
                    {
                        // root is single node
                        if (p._root.Next == null)
                        {
                            p._root.Previous = p._root;
                        }
                        else if (p._root.Previous == this)
                        {
                            p._root.Previous = prev;
                        }
                    }
                }
            }
        }
    }
}