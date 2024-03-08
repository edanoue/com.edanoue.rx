// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
#if SUPPORT_TIME_PROVIDER
using System;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_Debounce
    {
        [Test]
        public void Debounce()
        {
            var publisher = new Subject<int>();
            var result = 0;
            using var list = publisher.Debounce(TimeSpan.FromSeconds(3)).Subscribe(x => result = x);

            publisher.OnNext(1);
            Assert.That(result, Is.EqualTo(0));
            publisher.OnNext(10);
            Assert.That(result, Is.EqualTo(0));
            publisher.OnNext(100);
            Assert.That(result, Is.EqualTo(0));
        }
    }
}
#endif