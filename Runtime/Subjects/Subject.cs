// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Threading;
using Edanoue.Rx.Collections;

namespace Edanoue.Rx
{
    public sealed class Subject<T> : Observable<T>, ISubject<T>, IDisposable
    {
        private CompleteState              _completeState; // struct(int, IntPtr)
        private FreeListCore<Subscription> _list; // struct(array, int)

        public Subject()
        {
            // use self as gate(reduce memory usage), this is slightly dangerous so don't lock this in user.
            _list = new FreeListCore<Subscription>(this);
        }

        public bool IsDisposed => _completeState.IsDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        public void OnNext(T value)
        {
            if (_completeState.IsCompleted)
            {
                return;
            }

            foreach (var subscription in _list.AsSpan())
            {
                subscription?.Observer.OnNext(value);
            }
        }

        public void OnErrorResume(Exception error)
        {
            if (_completeState.IsCompleted)
            {
                return;
            }

            foreach (var subscription in _list.AsSpan())
            {
                subscription?.Observer.OnErrorResume(error);
            }
        }

        public void OnCompleted(Result result)
        {
            var status = _completeState.TrySetResult(result);
            if (status != CompleteState.ResultStatus.Done)
            {
                return; // already completed
            }

            foreach (var subscription in _list.AsSpan())
            {
                subscription?.Observer.OnCompleted(result);
            }
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            var result = _completeState.TryGetResult();
            if (result != null)
            {
                observer.OnCompleted(result.Value);
                return Disposable.Empty;
            }

            var subscription = new Subscription(this, observer); // create subscription and add observer to list.

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

        public void Dispose(bool callOnCompleted)
        {
            if (_completeState.TrySetDisposed(out var alreadyCompleted))
            {
                if (callOnCompleted && !alreadyCompleted)
                {
                    // not yet disposed so can call list iteration
                    foreach (var subscription in _list.AsSpan())
                    {
                        subscription?.Observer.OnCompleted();
                    }
                }

                _list.Dispose();
            }
        }

        private sealed class Subscription : IDisposable
        {
            private readonly int         _removeKey;
            public readonly  Observer<T> Observer;
            private          Subject<T>? _parent;

            public Subscription(Subject<T> parent, Observer<T> observer)
            {
                _parent = parent;
                Observer = observer;
                parent._list.Add(this, out _removeKey); // for the thread-safety, add and set removeKey in same lock.
            }

            public void Dispose()
            {
                // _parent に null を代入する, 以前が nullではない場合 Remove が呼ばれる
                var oldParent = Interlocked.Exchange(ref _parent, null);
                if (oldParent != null)
                {
                    // removeKey is index, will reuse if remove completed so only allows to call from here and must not call twice.
                    oldParent._list.Remove(_removeKey);
                }
            }
        }
    }
}