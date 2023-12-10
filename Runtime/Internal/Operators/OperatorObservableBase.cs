// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Operators
{
    internal abstract class OperatorObservableBase<T> : IObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            var subscription = new SingleAssignmentDisposable();
            subscription.Disposable = SubscribeInternal(observer, subscription);
            return subscription;
        }

        protected abstract IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel);
    }
}