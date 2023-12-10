// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Edanoue.Rx
{
    public interface IReadOnlyReactiveProperty<out T> : IObservable<T>
    {
        T Value { get; }
    }

    public interface IReactiveProperty<T> : IReadOnlyReactiveProperty<T>
    {
        new T Value { get; set; }
    }

    internal interface IObserverLinkedList<T>
    {
        internal void UnsubscribeNode(ObserverNode<T> node);
    }

    internal sealed class ObserverNode<T> : IObserver<T>, IDisposable
    {
        private readonly IObserver<T>            _observer;
        private          IObserverLinkedList<T>? _list;

        public ObserverNode(IObserverLinkedList<T> list, IObserver<T> observer)
        {
            _list = list;
            _observer = observer;
        }

        public ObserverNode<T>? Previous { get; internal set; }
        public ObserverNode<T>? Next { get; internal set; }

        public void Dispose()
        {
            var sourceList = Interlocked.Exchange(ref _list, null);
            sourceList?.UnsubscribeNode(this);
        }

        public void OnNext(T value)
        {
            _observer.OnNext(value);
        }

        public void OnError(Exception error)
        {
            _observer.OnError(error);
        }

        public void OnCompleted()
        {
            _observer.OnCompleted();
        }
    }

    public class ReactiveProperty<T> : IReactiveProperty<T>, IObserverLinkedList<T>, IDisposable
        where T : IEquatable<T>
    {
        private bool             _isDisposed;
        private ObserverNode<T>? _last;
        private ObserverNode<T>? _root;
        private T                _value;

        public ReactiveProperty() : this(default!)
        {
        }

        public ReactiveProperty(T initialValue)
        {
            _value = initialValue;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IObserverLinkedList<T>.UnsubscribeNode(ObserverNode<T> node)
        {
            if (node == _root)
            {
                _root = node.Next;
            }

            if (node == _last)
            {
                _last = node.Previous;
            }

            if (node.Previous != null)
            {
                node.Previous.Next = node.Next;
            }

            if (node.Next != null)
            {
                node.Next.Previous = node.Previous;
            }
        }

        public T Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _value;
            set
            {
                if (_value.Equals(value))
                {
                    return;
                }

                _value = value;
                if (_isDisposed)
                {
                    return;
                }

                RaiseOnNext(ref value);
            }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (_isDisposed)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            // raise latest value on subscribe
            observer.OnNext(_value);

            // subscribe node, node as subscription.
            var next = new ObserverNode<T>(this, observer);

            if (_root == null)
            {
                _root = _last = next;
            }
            else
            {
                _last!.Next = next;
                next.Previous = _last;
                _last = next;
            }

            return next;
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            var node = _root;
            _root = _last = null;
            _isDisposed = true;

            while (node != null)
            {
                node.OnCompleted();
                node = node.Next;
            }
        }

        private void RaiseOnNext(ref T value)
        {
            var node = _root;
            while (node != null)
            {
                node.OnNext(value);
                node = node.Next;
            }
        }
    }
}