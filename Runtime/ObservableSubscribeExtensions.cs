// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using Edanoue.Rx.Internal;

namespace Edanoue.Rx
{
    public static class ObservableSubscribeExtensions
    {
        public static IDisposable Subscribe<T>(this Observable<T> source)
        {
            return source.Subscribe(new NopObserver<T>());
        }

        public static IDisposable Subscribe<T>(this Observable<T> source, Action<T> onNext)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext, ObservableSystem.GetUnhandledExceptionHandler(),
                Stubs.HandleResult));
        }

        public static IDisposable Subscribe<T>(this Observable<T> source, Action<T> onNext, Action<Result> onCompleted)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext, ObservableSystem.GetUnhandledExceptionHandler(),
                onCompleted));
        }

        public static IDisposable Subscribe<T>(this Observable<T> source, Action<T> onNext,
            Action<Exception> onErrorResume, Action<Result> onCompleted)
        {
            return source.Subscribe(new AnonymousObserver<T>(onNext, onErrorResume, onCompleted));
        }

        // with state

        public static IDisposable Subscribe<T, TState>(this Observable<T> source, TState state,
            Action<T, TState> onNext)
        {
            return source.Subscribe(new AnonymousObserver<T, TState>(onNext, Stubs<TState>.HandleException,
                Stubs<TState>.HandleResult, state));
        }

        public static IDisposable Subscribe<T, TState>(this Observable<T> source, TState state,
            Action<T, TState> onNext, Action<Result, TState> onCompleted)
        {
            return source.Subscribe(new AnonymousObserver<T, TState>(onNext, Stubs<TState>.HandleException, onCompleted,
                state));
        }

        public static IDisposable Subscribe<T, TState>(this Observable<T> source, TState state,
            Action<T, TState> onNext, Action<Exception, TState> onErrorResume, Action<Result, TState> onCompleted)
        {
            return source.Subscribe(new AnonymousObserver<T, TState>(onNext, onErrorResume, onCompleted, state));
        }
    }

    internal sealed class NopObserver<T> : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            ObservableSystem.GetUnhandledExceptionHandler().Invoke(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            if (result.IsFailure)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(result.Exception!);
            }
        }
    }

    internal sealed class AnonymousObserver<T> : Observer<T>
    {
        private readonly Action<Result>    _onCompleted;
        private readonly Action<Exception> _onErrorResume;
        private readonly Action<T>         _onNext;

        public AnonymousObserver(Action<T> onNext, Action<Exception> onErrorResume, Action<Result> onCompleted)
        {
            _onNext = onNext;
            _onErrorResume = onErrorResume;
            _onCompleted = onCompleted;
        }

        protected override void OnNextCore(T value)
        {
            _onNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            _onErrorResume(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            _onCompleted(complete);
        }
    }

    internal sealed class AnonymousObserver<T, TState> : Observer<T>
    {
        private readonly Action<Result, TState>    _onCompleted;
        private readonly Action<Exception, TState> _onErrorResume;
        private readonly Action<T, TState>         _onNext;
        private readonly TState                    _state;

        public AnonymousObserver(Action<T, TState> onNext, Action<Exception, TState> onErrorResume,
            Action<Result, TState> onCompleted, TState state)
        {
            _onNext = onNext;
            _onErrorResume = onErrorResume;
            _onCompleted = onCompleted;
            _state = state;
        }

        protected override void OnNextCore(T value)
        {
            _onNext(value, _state);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            _onErrorResume(error, _state);
        }

        protected override void OnCompletedCore(Result complete)
        {
            _onCompleted(complete, _state);
        }
    }
}