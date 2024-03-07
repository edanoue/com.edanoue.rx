// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_TakeWhile
    {
        [Test]
        public void TakeWhile1()
        {
            using var a = new Subject<int>();
            var list = new List<int>();
            var isCompleted = false;
            using var subscription = a
                .TakeWhile(x => x * 2 <= 4)
                .Subscribe(list.Add, r => { isCompleted = true; });

            a.OnNext(1); // 2
            a.OnNext(2); // 4
            CollectionAssert.AreEqual(list, new[] { 1, 2 });
            Assert.IsFalse(isCompleted);

            a.OnNext(3); // 6 => Completed
            CollectionAssert.AreEqual(list, new[] { 1, 2 });
            Assert.IsTrue(isCompleted);
        }

        [Test]
        public void TakeWhile2()
        {
            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a
                .TakeWhile((x, i) => x * i < 2)
                .Subscribe(list.Add);

            a.OnNext(1); // 0
            a.OnNext(1); // 1
            CollectionAssert.AreEqual(list, new[] { 1, 1 });

            a.OnNext(1); // 2 => Completed
            a.OnNext(1); // 3
            CollectionAssert.AreEqual(list, new[] { 1, 1 });
        }
    }
}