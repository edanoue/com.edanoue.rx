// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_Where
    {
        [Test]
        [Category("Normal")]
        public void WhereNoIndex1()
        {
            using var a = new Subject<int>();

            var list = new List<int>();
            using var subscription = a.Where(x => x % 3 == 0).Subscribe(x => list.Add(x));

            a.OnNext(3);
            a.OnNext(5);
            a.OnNext(7);
            a.OnNext(9);
            a.OnNext(300);
            a.OnCompleted();

            CollectionAssert.AreEqual(list, new[] { 3, 9, 300 });
        }

        [Test]
        [Category("Normal")]
        public void WhereNoIndex2()
        {
            using var a = new Subject<int>();

            var list = new List<int>();
            using var subscription = a
                .Where(x => x % 3 == 0)
                .Where(x => x % 5 == 0)
                .Subscribe(list, (x, l) => l.Add(x));

            a.OnNext(3);
            a.OnNext(5);
            a.OnNext(7);
            a.OnNext(9);
            a.OnNext(300);
            a.OnCompleted();

            CollectionAssert.AreEqual(list, new[] { 300 });
        }

        [Test]
        [Category("Normal")]
        public void WhereWithIndex1()
        {
            using var a = new Subject<int>();

            var list = new List<int>();
            using var subscription = a.Where((x, i) => (x + i) % 3 == 0).Subscribe(x => list.Add(x));

            a.OnNext(3); // 3 + 0
            a.OnNext(5); // 5 + 1
            a.OnNext(7); // 7 + 2
            a.OnNext(9); // 9 + 3
            a.OnNext(300); // 300 + 4
            a.OnCompleted();

            CollectionAssert.AreEqual(list, new[] { 3, 5, 7, 9 });
        }

        [Test]
        [Category("Normal")]
        public void WhereWithIndex2()
        {
            using var a = new Subject<int>();

            var list = new List<int>();
            using var subscription =
                a.Where((x, i) => (x + i) % 3 == 0).Where(x => x % 5 == 0).Subscribe(x => list.Add(x));

            a.OnNext(3); // 3 + 0
            a.OnNext(5); // 5 + 1
            a.OnNext(7); // 7 + 2
            a.OnNext(9); // 9 + 3
            a.OnNext(300); // 300 + 4
            a.OnCompleted();

            CollectionAssert.AreEqual(list, new[] { 5 });
        }
    }
}