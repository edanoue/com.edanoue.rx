// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_DistinctUntilChanged
    {
        [Test]
        public void DistinctUntilChanged()
        {
            var source = new Subject<int>();
            var result = new List<int>();

            using var list = source.DistinctUntilChanged().Subscribe(result.Add);

            source.OnNext(1);
            source.OnNext(2);
            source.OnNext(3);
            source.OnNext(3);
            source.OnNext(2);
            source.OnNext(1);

            CollectionAssert.AreEqual(result, new[] { 1, 2, 3, 2, 1 });
        }
    }
}