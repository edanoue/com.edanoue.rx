// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace Edanoue.Rx
{
    public sealed class CompositeDisposable : ICollection<IDisposable>, IDisposable
    {
        private const    int                _SHRINK_THRESHOLD = 64;
        private readonly object             _gate             = new();
        private          bool               _isDisposed;
        private          List<IDisposable?> _list;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Reactive.Disposables.CompositeDisposable" /> class with no
        /// disposables contained by it initially.
        /// </summary>
        public CompositeDisposable()
        {
            _list = new List<IDisposable?>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Reactive.Disposables.CompositeDisposable" /> class with the
        /// specified number of disposables.
        /// </summary>
        /// <param name="capacity">The number of disposables that the new CompositeDisposable can initially store.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity" /> is less than zero.</exception>
        public CompositeDisposable(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _list = new List<IDisposable?>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Reactive.Disposables.CompositeDisposable" /> class from a group
        /// of disposables.
        /// </summary>
        /// <param name="disposables">Disposables that will be disposed together.</param>
        /// <exception cref="ArgumentNullException"><paramref name="disposables" /> is null.</exception>
        public CompositeDisposable(params IDisposable[] disposables)
        {
            _list = new List<IDisposable?>(disposables);
            Count = _list.Count;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Reactive.Disposables.CompositeDisposable" /> class from a group
        /// of disposables.
        /// </summary>
        /// <param name="disposables">Disposables that will be disposed together.</param>
        /// <exception cref="ArgumentNullException"><paramref name="disposables" /> is null.</exception>
        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _list = new List<IDisposable?>(disposables);
            Count = _list.Count;
        }

        /// <summary>
        /// Gets the number of disposables contained in the CompositeDisposable.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Adds a disposable to the CompositeDisposable or disposes the disposable if the CompositeDisposable is disposed.
        /// </summary>
        /// <param name="item">Disposable to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
        public void Add(IDisposable item)
        {
            bool shouldDispose;

            lock (_gate)
            {
                shouldDispose = _isDisposed;
                if (!_isDisposed)
                {
                    _list.Add(item);
                    Count++;
                }
            }

            if (shouldDispose)
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// Removes and disposes the first occurrence of a disposable from the CompositeDisposable.
        /// </summary>
        /// <param name="item">Disposable to remove.</param>
        /// <returns>true if found; false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
        public bool Remove(IDisposable item)
        {
            var shouldDispose = false;

            lock (_gate)
            {
                if (!_isDisposed)
                {
                    //
                    // List<T> doesn't shrink the size of the underlying array but does collapse the array
                    // by copying the tail one position to the left of the removal index. We don't need
                    // index-based lookup but only ordering for sequential disposal. So, instead of spending
                    // cycles on the Array.Copy imposed by Remove, we use a null sentinel value. We also
                    // do manual Swiss cheese detection to shrink the list if there's a lot of holes in it.
                    //
                    var i = _list.IndexOf(item);
                    if (i >= 0)
                    {
                        shouldDispose = true;
                        _list[i] = null;
                        Count--;

                        if (_list.Capacity > _SHRINK_THRESHOLD && Count < _list.Capacity / 2)
                        {
                            var old = _list;
                            _list = new List<IDisposable?>(_list.Capacity / 2);

                            foreach (var d in old)
                            {
                                if (d != null)
                                {
                                    _list.Add(d);
                                }
                            }
                        }
                    }
                }
            }

            if (shouldDispose)
            {
                item.Dispose();
            }

            return shouldDispose;
        }

        /// <summary>
        /// Removes and disposes all disposables from the CompositeDisposable, but does not dispose the CompositeDisposable.
        /// </summary>
        public void Clear()
        {
            IDisposable?[]? currentDisposables;
            lock (_gate)
            {
                currentDisposables = _list.ToArray();
                _list.Clear();
                Count = 0;
            }

            foreach (ref readonly var d in currentDisposables.AsSpan())
            {
                d?.Dispose();
            }
        }

        /// <summary>
        /// Determines whether the CompositeDisposable contains a specific disposable.
        /// </summary>
        /// <param name="item">Disposable to search for.</param>
        /// <returns>true if the disposable was found; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
        public bool Contains(IDisposable item)
        {
            lock (_gate)
            {
                return _list.Contains(item);
            }
        }

        /// <summary>
        /// Copies the disposables contained in the CompositeDisposable to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">Array to copy the contained disposables to.</param>
        /// <param name="arrayIndex">Target index at which to copy the first disposable of the group.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex" /> is less than zero. -or -
        /// <paramref name="arrayIndex" /> is larger than or equal to the array length.
        /// </exception>
        public void CopyTo(IDisposable[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            lock (_gate)
            {
                var disArray = new List<IDisposable>();
                foreach (var item in _list)
                {
                    if (item != null)
                    {
                        disArray.Add(item);
                    }
                }

                Array.Copy(disArray.ToArray(), 0, array, arrayIndex, array.Length - arrayIndex);
            }
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Returns an enumerator that iterates through the CompositeDisposable.
        /// </summary>
        /// <returns>An enumerator to iterate over the disposables.</returns>
        public IEnumerator<IDisposable> GetEnumerator()
        {
            var res = new List<IDisposable>();

            lock (_gate)
            {
                foreach (var d in _list)
                {
                    if (d != null)
                    {
                        res.Add(d);
                    }
                }
            }

            return res.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the CompositeDisposable.
        /// </summary>
        /// <returns>An enumerator to iterate over the disposables.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Disposes all disposables in the group and removes them from the group.
        /// </summary>
        public void Dispose()
        {
            IDisposable?[]? currentDisposables = null;

            lock (_gate)
            {
                if (!_isDisposed)
                {
                    _isDisposed = true;
                    currentDisposables = _list.ToArray();
                    _list.Clear();
                    Count = 0;
                }
            }

            if (currentDisposables is null) // Already Disposed
            {
                return;
            }

            foreach (ref readonly var d in currentDisposables.AsSpan())
            {
                d?.Dispose();
            }
        }
    }
}