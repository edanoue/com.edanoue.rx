// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_DistinctUntilChangedBy
    {
        [Test]
        public void DistinctUntilChangedBy()
        {
            var source = new Subject<(int, int)>();
            var result = new List<(int, int)>();

            using var list = source.DistinctUntilChangedBy(static x => x.Item1).Subscribe(result.Add);

            source.OnNext((1, 10));
            source.OnNext((2, 20));
            source.OnNext((3, 30));
            source.OnNext((3, 300));
            source.OnNext((2, 200));
            source.OnNext((1, 100));

            CollectionAssert.AreEqual(result, new[] { (1, 10), (2, 20), (3, 30), (2, 200), (1, 100) });
        }
    }
}