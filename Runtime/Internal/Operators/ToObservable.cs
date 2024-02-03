// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections.Generic;

namespace Edanoue.Rx.Operators
{
    internal class ToObservableObservable<T> : OperatorObservableBase<T>
    {
        private readonly IEnumerable<T> _source;

        public ToObservableObservable(IEnumerable<T> source)
        {
            _source = source;
        }

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            return new ToObservable(this, observer, cancel).Run();
        }

        private class ToObservable : OperatorObserverBase<T, T>
        {
            private readonly ToObservableObservable<T> _parent;

            public ToObservable(ToObservableObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(
                observer, cancel)
            {
                _parent = parent;
            }

            public IDisposable Run()
            {
                IEnumerator<T> e;
                try
                {
                    e = _parent._source.GetEnumerator();
                }
                catch (Exception exception)
                {
                    OnError(exception);
                    return Disposable.Empty;
                }

                // Note: (南) Immediatly で実行する

                // if (_parent.scheduler == Scheduler.Immediate)
                // {
                while (true)
                {
                    bool hasNext;
                    var current = default(T);
                    try
                    {
                        hasNext = e.MoveNext();
                        if (hasNext)
                        {
                            current = e.Current;
                        }
                    }
                    catch (Exception ex)
                    {
                        e.Dispose();
                        try
                        {
                            Observer.OnError(ex);
                        }
                        finally
                        {
                            Dispose();
                        }

                        break;
                    }

                    if (hasNext)
                    {
                        Observer.OnNext(current!);
                    }
                    else
                    {
                        e.Dispose();
                        try
                        {
                            Observer.OnCompleted();
                        }
                        finally
                        {
                            Dispose();
                        }

                        break;
                    }
                }

                return Disposable.Empty;
                // }
            }

            public override void OnNext(T value)
            {
                // do nothing
            }

            public override void OnError(Exception error)
            {
                try
                {
                    Observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public override void OnCompleted()
            {
                try
                {
                    Observer.OnCompleted();
                }
                finally
                {
                    Dispose();
                }
            }
        }
    }
}