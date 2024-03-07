// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_DisposableBag
    {
        private int _value;

        [Test]
        [Category("Normal")]
        public void Test()
        {
            _value = 0;
            var bag = new DisposableBag();

            var s1 = new Subject<int>();
            s1.AddTo(ref bag);

            using var subscription = s1.Subscribe(this, (x, t) => t._value = x);

            s1.OnNext(1);
            Assert.That(_value, Is.EqualTo(1));

            // Dispose bag
            bag.Dispose();

            Assert.Throws<ObjectDisposedException>(() =>
            {
                s1.OnNext(2); // Subject is disposed
            });
        }
    }
}