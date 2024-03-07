// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Edanoue.Rx
{
    public readonly struct Result
    {
        public static Result Success => default;

        public static Result Failure(Exception exception)
        {
            return new Result(exception);
        }

        public Exception? Exception { get; }

        // [MemberNotNullWhen(false, nameof(Exception))]
        public bool IsSuccess => Exception is null;

        // [MemberNotNullWhen(true, nameof(Exception))]
        public bool IsFailure => Exception is not null;

        public Result(Exception exception)
        {
            Exception = exception;
        }

        public void TryThrow()
        {
            if (IsFailure)
            {
                ExceptionDispatchInfo.Capture(Exception!).Throw();
            }
        }

        public override string ToString()
        {
            if (IsSuccess)
            {
                return "Success";
            }

            return $"Failure{{{Exception!.Message}}}";
        }
    }

    // thread-safety state for Subject.
    internal struct CompleteState
    {
        internal enum ResultStatus
        {
            Done,
            AlreadySuccess,
            AlreadyFailed
        }

        private const int        _NOT_COMPLETED     = 0;
        private const int        _COMPLETED_SUCCESS = 1;
        private const int        _COMPLETED_FAILURE = 2;
        private const int        _DISPOSED          = 3;
        private       int        _completeState;
        private       Exception? _error;

        public ResultStatus TrySetResult(Result result)
        {
            int field;
            if (result.IsSuccess)
            {
                field = Interlocked.CompareExchange(ref _completeState, _COMPLETED_SUCCESS,
                    _NOT_COMPLETED); // try set success
            }
            else
            {
                field = Interlocked.CompareExchange(ref _completeState, _COMPLETED_FAILURE,
                    _NOT_COMPLETED); // try set failure
                Volatile.Write(ref _error, result.Exception); // set failure immmediately(but not locked).
            }

            switch (field)
            {
                case _NOT_COMPLETED:
                    return ResultStatus.Done;
                case _COMPLETED_SUCCESS:
                    return ResultStatus.AlreadySuccess;
                case _COMPLETED_FAILURE:
                    return ResultStatus.AlreadyFailed;
                case _DISPOSED:
                    ThrowObjectDisposedException();
                    break;
            }

            return ResultStatus.Done; // not here.
        }

        public bool TrySetDisposed(out bool alreadyCompleted)
        {
            var field = Interlocked.Exchange(ref _completeState, _DISPOSED);
            switch (field)
            {
                case _NOT_COMPLETED:
                    alreadyCompleted = false;
                    return true;
                case _COMPLETED_SUCCESS:
                case _COMPLETED_FAILURE:
                    alreadyCompleted = true;
                    return true;
                case _DISPOSED:
                    break;
            }

            alreadyCompleted = false;
            return false;
        }

        public bool IsCompleted
        {
            get
            {
                var currentState = Volatile.Read(ref _completeState);
                switch (currentState)
                {
                    case _NOT_COMPLETED:
                        return false;
                    case _COMPLETED_SUCCESS:
                        return true;
                    case _COMPLETED_FAILURE:
                        return true;
                    case _DISPOSED:
                        ThrowObjectDisposedException();
                        break;
                }

                return false; // not here.
            }
        }

        public bool IsDisposed => Volatile.Read(ref _completeState) == _DISPOSED;

        public Result? TryGetResult()
        {
            var currentState = Volatile.Read(ref _completeState);

            switch (currentState)
            {
                case _NOT_COMPLETED:
                    return null;
                case _COMPLETED_SUCCESS:
                    return Result.Success;
                case _COMPLETED_FAILURE:
                    return Result.Failure(GetException());
                case _DISPOSED:
                    ThrowObjectDisposedException();
                    break;
            }

            return null; // not here.
        }

        // be careful to use, this method need to call after ResultStatus.AlreadyFailed.
        private Exception GetException()
        {
            var error = Volatile.Read(ref _error);
            if (error != null)
            {
                return error;
            }

            var spinner = new SpinWait();
            do
            {
                spinner.SpinOnce();
                error = Volatile.Read(ref _error);
            } while (error == null);

            return error;
        }

        private static void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException("");
        }
    }
}