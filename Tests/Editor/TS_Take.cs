// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_Take
    {
        [Test]
        public void Take1()
        {
            using var a = new Subject<int>();
            var list = new List<int>();
            var isCompleted = false;
            using var subscription = a
                .Take(3)
                .Subscribe(list.Add, () => { isCompleted = true; });

            a.OnNext(1);
            a.OnNext(2);
            CollectionAssert.AreEqual(list, new[] { 1, 2 });
            Assert.IsFalse(isCompleted);

            a.OnNext(3);
            CollectionAssert.AreEqual(list, new[] { 1, 2, 3 });
            Assert.IsTrue(isCompleted);

            a.OnNext(4);
            CollectionAssert.AreEqual(list, new[] { 1, 2, 3 });
        }

        [Test]
        public void Take2()
        {
            // Take が連続している場合, 値が小さい方が優先される
            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a
                .Take(5)
                .Take(3)
                .Subscribe(list.Add);

            a.OnNext(1);
            a.OnNext(2);
            a.OnNext(3);
            a.OnNext(4);
            CollectionAssert.AreEqual(list, new[] { 1, 2, 3 });
        }

        [Test]
        public void Take3()
        {
            // Take が 0 の場合, Empty が返る
            using var a = new Subject<int>();
            var list = new List<int>();
            var isCompleted = false;
            using var subscription = a
                .Take(0)
                .Subscribe(list.Add, () => { isCompleted = true; });

            Assert.IsTrue(isCompleted);
            a.OnNext(1);
            a.OnNext(2);
            CollectionAssert.AreEqual(list, new int[] { });
        }

        [Test]
        public void Take4()
        {
            // Take が 0 の場合, Empty が返る
            using var a = new Subject<int>();
            var list = new List<int>();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using var subscription = a
                    .Take(-1)
                    .Subscribe(list.Add);
            });
        }
    }
}