// Copyright Edanoue, Inc. All Rights Reserved.

using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_SkipWhile
    {
        [Test]
        public void SkipWhile1()
        {
            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a.SkipWhile(x => x <= 2).Subscribe(x => list.Add(x));

            a.OnNext(1);
            a.OnNext(2);
            CollectionAssert.AreEqual(list, new int[] { });

            a.OnNext(3);
            a.OnNext(4);
            a.OnNext(5);
            CollectionAssert.AreEqual(list, new[] { 3, 4, 5 });

            // 一度 SkipWhile の条件を満たした後はすべて通す
            a.OnNext(2);
            a.OnNext(5);
            CollectionAssert.AreEqual(list, new[] { 3, 4, 5, 2, 5 });
        }

        [Test]
        public void SkipWhile2()
        {
            // SkipWhile() with index predicate

            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a
                .SkipWhile((value, index) => value + index != 5)
                .Subscribe(x => list.Add(x));

            a.OnNext(1); // 1 + 0
            a.OnNext(2); // 2 + 1
            CollectionAssert.AreEqual(list, new int[] { });

            a.OnNext(3); // 3 + 2 => PASS
            CollectionAssert.AreEqual(list, new[] { 3 });

            // 一度 SkipWhile の条件を満たした後はすべて通す
            a.OnNext(1);
            a.OnNext(2);
            CollectionAssert.AreEqual(list, new[] { 3, 1, 2 });
        }

        [Test]
        public void SkipWhile3()
        {
            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a
                .SkipWhile(x => x != 6)
                .SkipWhile(x => x != 2)
                .Subscribe(x => list.Add(x));

            a.OnNext(2);
            CollectionAssert.AreEqual(list, new int[] { });

            a.OnNext(6);
            a.OnNext(4);
            CollectionAssert.AreEqual(list, new int[] { });

            a.OnNext(2);
            a.OnNext(5);
            CollectionAssert.AreEqual(list, new[] { 2, 5 });
        }
    }
}