// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;

namespace Edanoue.Rx
{
    public struct DisposableBag : IDisposable
    {
        private IDisposable[]? _items;
        private bool           _isDisposed;
        private int            _count;

        public DisposableBag(int capacity)
        {
            _isDisposed = false;
            _count = 0;
            _items = new IDisposable[capacity];
        }

        public void Add(IDisposable item)
        {
            if (_isDisposed)
            {
                item.Dispose();
                return;
            }

            if (_items == null)
            {
                _items = new IDisposable[4];
            }
            else if (_count == _items.Length)
            {
                Array.Resize(ref _items, _count * 2);
            }

            _items[_count++] = item;
        }

        public void Clear()
        {
            if (_items != null)
            {
                var span = _items.AsSpan(0, _count);
                foreach (ref readonly var item in span)
                {
                    item?.Dispose();
                }

                _items = null;
                _count = 0;
            }
        }

        public void Dispose()
        {
            Clear();
            _isDisposed = true;
        }
    }
}