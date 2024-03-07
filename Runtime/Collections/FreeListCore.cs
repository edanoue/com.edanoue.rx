// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Edanoue.Rx.Collections
{
    [StructLayout(LayoutKind.Auto)]
    public struct FreeListCore<T>
        where T : class
    {
        private readonly object _gate;
        private          T?[]?  _values;
        private          int    _lastIndex;

        public FreeListCore(object gate)
        {
            // don't create values at initialize
            _gate = gate;
            _values = null;
            _lastIndex = -1;
        }

        public bool IsDisposed => _lastIndex == -2;

        public ReadOnlySpan<T> AsSpan()
        {
            var last = Volatile.Read(ref _lastIndex);
            var xs = Volatile.Read(ref _values);
            if (xs == null)
            {
                return ReadOnlySpan<T?>.Empty;
            }

            return xs.AsSpan(0, last + 1);
        }

        public void Add(T item, out int removeKey)
        {
            lock (_gate)
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(typeof(FreeListCore<T>).FullName);
                }

                if (_values == null)
                {
                    _values = new T[1]; // initial size is 1.
                }

                // try find blank
                var index = FindNullIndex(_values);
                if (index == -1)
                {
                    // full, 1, 4, 6,...resize(x1.5)
                    var len = _values.Length;
                    var newValues = len == 1 ? new T[4] : new T[len + len / 2];
                    Array.Copy(_values, newValues, len);
                    Volatile.Write(ref _values, newValues);
                    index = len;
                }

                _values[index] = item;
                if (_lastIndex < index)
                {
                    Volatile.Write(ref _lastIndex, index);
                }

                removeKey = index; // index is remove key.
            }
        }

        public void Remove(int index)
        {
            lock (_gate)
            {
                if (_values == null)
                {
                    return;
                }

                if (index < _values.Length)
                {
                    ref var v = ref _values[index];
                    if (v == null)
                    {
                        throw new KeyNotFoundException($"key index {index} is not found.");
                    }

                    v = null;
                    if (index == _lastIndex)
                    {
                        Volatile.Write(ref _lastIndex, FindLastNonNullIndex(_values, index));
                    }
                }
            }
        }

        public bool RemoveSlow(T value)
        {
            lock (_gate)
            {
                if (_values == null)
                {
                    return false;
                }

                if (_lastIndex < 0)
                {
                    return false;
                }

                var index = -1;
                var span = _values.AsSpan(0, _lastIndex + 1);
                for (var i = 0; i < span.Length; i++)
                {
                    if (span[i] == value)
                    {
                        index = i;
                        break;
                    }
                }

                if (index != -1)
                {
                    Remove(index);
                    return true;
                }
            }

            return false;
        }

        public void Clear(bool removeArray)
        {
            lock (_gate)
            {
                if (_lastIndex >= 0)
                {
                    _values.AsSpan(0, _lastIndex + 1).Clear();
                }

                if (removeArray)
                {
                    _values = null;
                }

                if (_lastIndex != -2)
                {
                    _lastIndex = -1;
                }
            }
        }

        public void Dispose()
        {
            lock (_gate)
            {
                _values = null;
                _lastIndex = -2; // -2 is disposed.
            }
        }

#if NET6_0_OR_GREATER
        static int FindNullIndex(T?[] target)
        {
            var span = MemoryMarshal.CreateReadOnlySpan(
                ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), target.Length);
            return span.IndexOf(IntPtr.Zero);
        }

#else
        private static unsafe int FindNullIndex(T?[] target)
        {
            ref var head = ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetReference(target.AsSpan()));
            fixed (void* p = &head)
            {
                var span = new ReadOnlySpan<IntPtr>(p, target.Length);

#if NETSTANDARD2_1
                return span.IndexOf(IntPtr.Zero);
#else
                for (int i = 0; i < span.Length; i++)
                {
                    if (span[i] == IntPtr.Zero) return i;
                }

                return -1;
#endif
            }
        }

#endif

#if NET8_0_OR_GREATER
        static int FindLastNonNullIndex(T?[] target, int lastIndex)
        {
            var span = MemoryMarshal.CreateReadOnlySpan(
                ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetArrayDataReference(target)), lastIndex); // without lastIndexed value.
            var index = span.LastIndexOfAnyExcept(IntPtr.Zero);
            return index; // return -1 is ok(means empty)
        }

#else

        private static unsafe int FindLastNonNullIndex(T?[] target, int lastIndex)
        {
            ref var head = ref Unsafe.As<T?, IntPtr>(ref MemoryMarshal.GetReference(target.AsSpan()));
            fixed (void* p = &head)
            {
                var span = new ReadOnlySpan<IntPtr>(p, lastIndex); // without lastIndexed value.

                for (var i = span.Length - 1; i >= 0; i--)
                {
                    if (span[i] != IntPtr.Zero)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

#endif
    }
}