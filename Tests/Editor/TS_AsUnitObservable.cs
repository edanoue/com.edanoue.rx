// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_AsUnitObservable
    {
        [Test]
        [Category("Normal")]
        public void AsUnitObservable()
        {
            using var s1 = new Subject<int>();

            var list = new List<Unit>();
            var isComplete = false;
            using var subscription = s1.AsUnitObservable().Subscribe(list.Add, () => isComplete = true);

            // Commit value
            s1.OnNext(10);

            // Compare list
            CollectionAssert.AreEqual(list, new[] { Unit.Default });

            // Check OnComplete
            s1.OnCompleted();
            Assert.That(isComplete, Is.True);
        }
    }
}