// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Operators
{
    internal class MergeObservable<T> : OperatorObservableBase<T>
    {
        private readonly IObservable<IObservable<T>> _sources;

        public MergeObservable(IObservable<IObservable<T>> sources)
        {
            _sources = sources;
        }

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            return new MergeOuterObserver(this, observer, cancel).Run();
        }

        private class MergeOuterObserver : OperatorObserverBase<IObservable<T>, T>
        {
            private readonly CompositeDisposable        _collectionDisposable;
            private readonly object                     _gate = new();
            private readonly SingleAssignmentDisposable _sourceDisposable;
            private          bool                       _isStopped;

            public MergeOuterObserver(MergeObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(
                observer, cancel)
            {
                _collectionDisposable = new CompositeDisposable();
                _sourceDisposable = new SingleAssignmentDisposable();
                _collectionDisposable.Add(_sourceDisposable);
                _sourceDisposable.Disposable = parent._sources.Subscribe(this);
            }

            public IDisposable Run()
            {
                return _collectionDisposable;
            }

            public override void OnNext(IObservable<T> value)
            {
                var disposable = new SingleAssignmentDisposable();
                _collectionDisposable.Add(disposable);
                var collectionObserver = new Merge(this, disposable);
                disposable.Disposable = value.Subscribe(collectionObserver);
            }

            public override void OnError(Exception error)
            {
                lock (_gate)
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
            }

            public override void OnCompleted()
            {
                _isStopped = true;
                if (_collectionDisposable.Count == 1)
                {
                    lock (_gate)
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
                else
                {
                    _sourceDisposable.Dispose();
                }
            }

            private class Merge : OperatorObserverBase<T, T>
            {
                private readonly IDisposable        _cancel;
                private readonly MergeOuterObserver _parent;

                public Merge(MergeOuterObserver parent, IDisposable cancel)
                    : base(parent.Observer, cancel)
                {
                    _parent = parent;
                    _cancel = cancel;
                }

                public override void OnNext(T value)
                {
                    lock (_parent._gate)
                    {
                        Observer.OnNext(value);
                    }
                }

                public override void OnError(Exception error)
                {
                    lock (_parent._gate)
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
                }

                public override void OnCompleted()
                {
                    _parent._collectionDisposable.Remove(_cancel);
                    if (_parent._isStopped && _parent._collectionDisposable.Count == 1)
                    {
                        lock (_parent._gate)
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
        }
    }
}