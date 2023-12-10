// Copyright Edanoue, Inc. All Rights Reserved.

using System;

namespace Edanoue.Rx.Internal
{
    public class ListObserver<T> : IObserver<T>
    {
        private readonly ImmutableList<IObserver<T>> _observers;

        public ListObserver(ImmutableList<IObserver<T>> observers)
        {
            _observers = observers;
        }

        public void OnCompleted()
        {
            var targetObservers = _observers.Data;
            foreach (ref var j in targetObservers.AsSpan())
            {
                j.OnCompleted();
            }
        }

        public void OnError(Exception error)
        {
            var targetObservers = _observers.Data;
            foreach (ref var A in targetObservers.AsSpan())
            {
                A.OnError(error);
            }
        }

        public void OnNext(T value)
        {
            var targetObservers = _observers.Data;
            foreach (ref var t in targetObservers.AsSpan())
            {
                t.OnNext(value);
            }
        }

        internal IObserver<T> Add(IObserver<T> observer)
        {
            return new ListObserver<T>(_observers.Add(observer));
        }

        internal IObserver<T> Remove(IObserver<T> observer)
        {
            var i = Array.IndexOf(_observers.Data, observer);
            if (i < 0)
            {
                return this;
            }

            if (_observers.Data.Length == 2)
            {
                return _observers.Data[1 - i];
            }

            return new ListObserver<T>(_observers.Remove(observer));
        }
    }

    public class NoOpObserver<T> : IObserver<T>
    {
        public static readonly NoOpObserver<T> Default = new();

        private NoOpObserver()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(T value)
        {
        }
    }

    public class ThrowObserver<T> : IObserver<T>
    {
        public static readonly ThrowObserver<T> Instance = new();

        private ThrowObserver()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            error.Throw();
        }

        public void OnNext(T value)
        {
        }
    }

    public class DisposedObserver<T> : IObserver<T>
    {
        public static readonly DisposedObserver<T> Default = new();

        private DisposedObserver()
        {
        }

        public void OnCompleted()
        {
            throw new ObjectDisposedException("");
        }

        public void OnError(Exception error)
        {
            throw new ObjectDisposedException("");
        }

        public void OnNext(T value)
        {
            throw new ObjectDisposedException("");
        }
    }
}