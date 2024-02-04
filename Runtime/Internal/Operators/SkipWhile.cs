// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx.Operators
{
    internal class SkipWhileObservable<T> : OperatorObservableBase<T>
    {
        private readonly Func<T, bool>?      _predicateNoIndex;
        private readonly Func<T, int, bool>? _predicateWithIndex;
        private readonly IObservable<T>      _source;

        public SkipWhileObservable(IObservable<T> source, Func<T, bool> predicate)
        {
            _source = source;
            _predicateNoIndex = predicate;
        }

        public SkipWhileObservable(IObservable<T> source, Func<T, int, bool> predicateWithIndex)
        {
            _source = source;
            _predicateWithIndex = predicateWithIndex;
        }

        protected override IDisposable SubscribeInternal(IObserver<T> observer, IDisposable cancel)
        {
            if (_predicateNoIndex is not null)
            {
                return new SkipWhileNoIndex(this, observer, cancel).Run();
            }

            return new SkipWhileWithIndex(this, observer, cancel).Run();
        }

        private class SkipWhileNoIndex : OperatorObserverBase<T, T>
        {
            private readonly SkipWhileObservable<T> _parent;
            private          bool                   _isBypass;

            public SkipWhileNoIndex(SkipWhileObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(
                observer, cancel)
            {
                _parent = parent;
            }

            public IDisposable Run()
            {
                return _parent._source.Subscribe(this);
            }

            public override void OnNext(T value)
            {
                if (!_isBypass)
                {
                    try
                    {
                        _isBypass = !_parent._predicateNoIndex!(value);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Observer.OnError(ex);
                        }
                        finally
                        {
                            Dispose();
                        }

                        return;
                    }

                    if (!_isBypass)
                    {
                        return;
                    }
                }

                Observer.OnNext(value);
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

        private class SkipWhileWithIndex : OperatorObserverBase<T, T>
        {
            private readonly SkipWhileObservable<T> _parent;
            private          int                    _index;
            private          bool                   _isBypass;

            public SkipWhileWithIndex(SkipWhileObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(
                observer, cancel)
            {
                _parent = parent;
            }

            public IDisposable Run()
            {
                return _parent._source.Subscribe(this);
            }

            public override void OnNext(T value)
            {
                if (!_isBypass)
                {
                    try
                    {
                        _isBypass = !_parent._predicateWithIndex!(value, _index++);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Observer.OnError(ex);
                        }
                        finally
                        {
                            Dispose();
                        }

                        return;
                    }

                    if (!_isBypass)
                    {
                        return;
                    }
                }

                Observer.OnNext(value);
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