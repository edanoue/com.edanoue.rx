// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace Edanoue.Rx
{
    [Serializable]
    public readonly struct Unit : IEquatable<Unit>
    {
        public static Unit Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        } = new();

        public bool Equals(Unit other)
        {
            return true;
        }

        public static bool operator ==(Unit first, Unit second)
        {
            return true;
        }

        public static bool operator !=(Unit first, Unit second)
        {
            return false;
        }

        public override bool Equals(object? obj)
        {
            return obj is Unit;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "()";
        }
    }
}