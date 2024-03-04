// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_DisposableBag
    {
        [Test]
        [Category("Normal")]
        public void Test()
        {
            var bag = new DisposableBag();

            var s1 = new Subject<int>();
            s1.AddTo(ref bag);
            var value = 0;

            using var subscription = s1.Subscribe(x => value = x);

            s1.OnNext(1);
            Assert.That(value, Is.EqualTo(1));

            // Dispose bag
            bag.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                s1.OnNext(2); // Subject is disposed
            });
        }
    }
}