// Copyright Edanoue, Inc. All Rights Reserved.

using System;
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_CombineLatest
    {
        [Test]
        [Category("Normal")]
        public void Test()
        {
            var source1 = new Subject<int>();
            var source2 = new Subject<string>();
            (int, string) result = default;
            var isCompleted = false;

            using var subscription = source1
                .CombineLatest(source2, ValueTuple.Create)
                .Subscribe(x => result = x,
                    _ => isCompleted = true
                );

            source1.OnNext(1);
            Assert.That(result, Is.EqualTo(default((int, string))));

            source1.OnNext(2);
            Assert.That(result, Is.EqualTo(default((int, string))));

            source2.OnNext("a");
            Assert.That(result, Is.EqualTo((2, "a")));

            source1.OnNext(3);
            Assert.That(result, Is.EqualTo((3, "a")));

            source2.OnNext("b");
            Assert.That(result, Is.EqualTo((3, "b")));

            source2.OnNext("c");
            Assert.That(result, Is.EqualTo((3, "c")));

            source1.OnCompleted();
            Assert.That(isCompleted, Is.False);

            source1.OnNext(4);
            source2.OnNext("d");
            Assert.That(result, Is.EqualTo((3, "d")));

            source2.OnCompleted();
            Assert.That(isCompleted, Is.True);
        }
    }
}