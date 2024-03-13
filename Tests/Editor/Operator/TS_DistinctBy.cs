// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_DistinctBy
    {
        [Test]
        public void DistinctBy()
        {
            var source = new Subject<(int, int)>();
            var result = new List<(int, int)>();

            using var list = source.DistinctBy(static x => x.Item1).Subscribe(result.Add);

            source.OnNext((1, 10));
            source.OnNext((2, 20));
            source.OnNext((3, 30));
            source.OnNext((1, 100));
            source.OnNext((2, 200));
            source.OnNext((3, 300));
            source.OnNext((4, 400));

            CollectionAssert.AreEqual(result, new[] { (1, 10), (2, 20), (3, 30), (4, 400) });
        }
    }
}