// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Threading;

namespace Edanoue.Rx
{
    public abstract class Observable<T>
    {
        /// <summary>
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(Observer<T> observer)
        {
            try
            {
                var subscription = SubscribeCore(observer);

                /*
                if (ObservableTracker.TryTrackActiveSubscription(subscription, 2, out var trackableDisposable))
                {
                    subscription = trackableDisposable;
                }
                */

                observer.SourceSubscription.Disposable = subscription;
                return observer; // return observer to make subscription chain.
            }
            catch
            {
                observer.Dispose(); // when SubscribeCore failed, auto detach caller observer
                throw;
            }
        }

        protected abstract IDisposable SubscribeCore(Observer<T> observer);
    }

    public abstract class Observer<T> : IDisposable
    {
        private int _calledOnCompleted;
        private int _disposed;

        /*
#if DEBUG
        [Obsolete("Only allow in Observable<T>.")]
#endif
        */
        internal SingleAssignmentDisposableCore SourceSubscription;

        public bool IsDisposed => Volatile.Read(ref _disposed) != 0;
        private bool IsCalledCompleted => Volatile.Read(ref _calledOnCompleted) != 0;

        // enable/disable auto dispose on completed.
        protected virtual bool AutoDisposeOnCompleted => true;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            DisposeCore(); // Dispose self
            SourceSubscription.Dispose(); // Dispose attached parent
        }

        public void OnNext(T value)
        {
            if (IsDisposed || IsCalledCompleted)
            {
                return;
            }

            try
            {
                OnNextCore(value);
            }
            catch (Exception ex)
            {
                OnErrorResume(ex);
            }
        }

        protected abstract void OnNextCore(T value);

        public void OnErrorResume(Exception error)
        {
            if (IsDisposed || IsCalledCompleted)
            {
                return;
            }

            try
            {
                OnErrorResumeCore(error);
            }
            catch (Exception ex)
            {
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
            }
        }

        protected abstract void OnErrorResumeCore(Exception error);

        public void OnCompleted(Result result)
        {
            if (Interlocked.Exchange(ref _calledOnCompleted, 1) != 0)
            {
                return;
            }

            if (IsDisposed)
            {
                return;
            }

            var disposeOnFinally = AutoDisposeOnCompleted;
            try
            {
                OnCompletedCore(result);
            }
            catch (Exception ex)
            {
                disposeOnFinally = true;
                ObservableSystem.GetUnhandledExceptionHandler().Invoke(ex);
            }
            finally
            {
                if (disposeOnFinally)
                {
                    Dispose();
                }
            }
        }

        protected abstract void OnCompletedCore(Result result);

        protected virtual void DisposeCore()
        {
        }
    }

    public static class ObserverExtensions
    {
        public static void OnCompleted<T>(this Observer<T> observer)
        {
            observer.OnCompleted(Result.Success);
        }

        public static void OnCompleted<T>(this Observer<T> observer, Exception exception)
        {
            observer.OnCompleted(Result.Failure(exception));
        }
    }
}