// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx
{
    public static class ObservableSystem
    {
        private static readonly Action<Exception> _unhandledException = DefaultUnhandledExceptionHandler;

        public static Action<Exception> GetUnhandledExceptionHandler()
        {
            return _unhandledException;
        }

        private static void DefaultUnhandledExceptionHandler(Exception exception)
        {
            Console.WriteLine($"EdaRx UnhandledException: {exception}");
        }
    }
}