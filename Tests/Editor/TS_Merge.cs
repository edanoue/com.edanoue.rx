// Copyright Edanoue, Inc. All Rights Reserved.

using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_Merge
    {
        [Test]
        [Category("Normal")]
        public void Merge1()
        {
            using var s1 = new Subject<int>();
            using var s2 = new Subject<int>();
            using var s3 = new Subject<int>();

            var list = new List<int>();
            var isComplete = false;
            using var subscription = Observable.Merge(s1, s2, s3).Subscribe(list.Add, r => isComplete = true);

            // Commit value
            s1.OnNext(10);
            s1.OnNext(20);
            s3.OnNext(100);
            s2.OnNext(50);

            // Compare list
            CollectionAssert.AreEqual(list, new[] { 10, 20, 100, 50 });

            // Check OnComplete

            Assert.That(isComplete, Is.False);

            s2.OnCompleted();
            Assert.That(isComplete, Is.False);

            s3.OnNext(500);
            s1.OnCompleted();
            Assert.That(isComplete, Is.False);
            CollectionAssert.AreEqual(list, new[] { 10, 20, 100, 50, 500 });

            s3.OnCompleted();
            Assert.That(isComplete, Is.True);
        }
    }
}