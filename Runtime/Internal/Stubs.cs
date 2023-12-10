// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx.Internal
{
    internal static class Stubs
    {
        public static readonly Action            NoOp  = () => { };
        public static readonly Action<Exception> Throw = ex => { ex.Throw(); };
    }
}