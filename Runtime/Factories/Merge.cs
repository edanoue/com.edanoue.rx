// Copyright Edanoue, Inc. All Rights Reserved.

using System;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public static partial class Observable
    {
        public static Observable<T> Merge<T>(params Observable<T>[] sources)
        {
            return new Merge<T>(sources);
        }

        public static Observable<T> Merge<T>(this IEnumerable<Observable<T>> sources)
        {
            return new Merge<T>(sources);
        }
    }

    public static partial class ObservableExtensions
    {
        public static Observable<T> Merge<T>(this Observable<T> source, Observable<T> second)
        {
            return new Merge<T>(new[] { source, second });
        }
    }


    internal sealed class Merge<T> : Observable<T>
    {
        private readonly IEnumerable<Observable<T>> _sources;

        public Merge(IEnumerable<Observable<T>> sources)
        {
            _sources = sources;
        }

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            var merge = new MergeInternal(observer);
            var builder = Disposable.CreateBuilder();

            var count = 0;
            foreach (var item in _sources)
            {
                item.Subscribe(new MergeObserver(merge)).AddTo(ref builder);
                count++;
            }

            merge.Disposable.Disposable = builder.Build();

            merge.SetSourceCount(count);

            return merge;
        }

        private sealed class MergeInternal : IDisposable
        {
            public readonly object      Gate = new();
            public readonly Observer<T> Observer;
            private         int         _completeCount;

            private int                            _sourceCount = -1; // not set yet.
            public  SingleAssignmentDisposableCore Disposable;

            public MergeInternal(Observer<T> observer)
            {
                Observer = observer;
                Disposable = new SingleAssignmentDisposableCore();
            }

            public void Dispose()
            {
                Disposable.Dispose();
            }

            public void SetSourceCount(int count)
            {
                lock (Gate)
                {
                    _sourceCount = count;
                    if (_sourceCount == _completeCount)
                    {
                        Observer.OnCompleted();
                        Dispose();
                    }
                }
            }

            // when all sources are completed, then this observer is completed
            public void TryPublishCompleted()
            {
                lock (Gate)
                {
                    _completeCount++;
                    if (_completeCount == _sourceCount)
                    {
                        Observer.OnCompleted();
                        Dispose();
                    }
                }
            }
        }

        private sealed class MergeObserver : Observer<T>
        {
            private readonly MergeInternal _parent;

            public MergeObserver(MergeInternal parent)
            {
                _parent = parent;
            }

            protected override void OnNextCore(T value)
            {
                lock (_parent.Gate)
                {
                    _parent.Observer.OnNext(value);
                }
            }

            protected override void OnErrorResumeCore(Exception error)
            {
                lock (_parent.Gate)
                {
                    _parent.Observer.OnErrorResume(error);
                }
            }

            protected override void OnCompletedCore(Result result)
            {
                if (result.IsFailure)
                {
                    // when error, publish OnCompleted immediately
                    lock (_parent.Gate)
                    {
                        _parent.Observer.OnCompleted(result);
                    }
                }
                else
                {
                    _parent.TryPublishCompleted();
                }
            }
        }
    }
}