// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx.Internal
{
    internal static class TimeSpanExtensions
    {
        public static TimeSpan Normalize(this TimeSpan timeSpan)
        {
            return timeSpan >= TimeSpan.Zero ? timeSpan : TimeSpan.Zero;
        }
    }
}