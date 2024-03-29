﻿// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_Skip
    {
        [Test]
        public void Skip1()
        {
            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a.Skip(3).Subscribe(list.Add);

            a.OnNext(1);
            a.OnNext(2);
            a.OnNext(3);
            CollectionAssert.AreEqual(list, new int[] { });

            a.OnNext(4);
            a.OnNext(5);
            a.OnNext(6);
            CollectionAssert.AreEqual(list, new[] { 4, 5, 6 });
        }

        [Test]
        public void Skip2()
        {
            // Check skip().skip() optimization
            using var a = new Subject<int>();
            var list = new List<int>();
            using var subscription = a.Skip(3).Skip(2).Subscribe(list.Add);

            a.OnNext(1);
            a.OnNext(2);
            a.OnNext(3);
            a.OnNext(4);
            a.OnNext(5);
            CollectionAssert.AreEqual(list, new int[] { });

            a.OnNext(6);
            a.OnNext(7);
            CollectionAssert.AreEqual(list, new[] { 6, 7 });
        }

        [Test]
        public void Skip3()
        {
            // Take が 0 の場合, Empty が返る
            using var a = new Subject<int>();
            var list = new List<int>();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                using var subscription = a
                    .Skip(-1)
                    .Subscribe(list.Add);
            });
        }
    }
}