// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
#if (NET_4_6 || NET_STANDARD_2_0)
using System.Runtime.ExceptionServices;
#endif

namespace Edanoue.Rx.Internal
{
    internal static class ExceptionExtensions
    {
        public static void Throw(this Exception exception)
        {
#if (NET_4_6 || NET_STANDARD_2_0)
            ExceptionDispatchInfo.Capture(exception).Throw();
#endif
            throw exception;
        }
    }
}