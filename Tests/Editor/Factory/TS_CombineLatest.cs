// Copyright Edanoue, Inc. All Rights Reserved.

using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_CombineLatestFactory
    {
        [Test]
        [Category("Normal")]
        public void Test()
        {
            var source1 = new Subject<int>();
            var source2 = new Subject<int>();
            int[] result = default;
            var isCompleted = false;

            using var subscription = Observable
                .CombineLatest(source1, source2)
                .Subscribe(x => result = x,
                    _ => isCompleted = true
                );

            source1.OnNext(1);
            Assert.That(result, Is.EqualTo(default(int[])));

            source1.OnNext(2);
            Assert.That(result, Is.EqualTo(default(int[])));

            source2.OnNext(1);
            Assert.That(result, Is.EqualTo(new[] { 2, 1 }));

            source1.OnNext(3);
            Assert.That(result, Is.EqualTo(new[] { 3, 1 }));

            source2.OnNext(2);
            Assert.That(result, Is.EqualTo(new[] { 3, 2 }));

            source2.OnNext(3);
            Assert.That(result, Is.EqualTo(new[] { 3, 3 }));

            source1.OnCompleted();
            Assert.That(isCompleted, Is.False);

            source1.OnNext(4);
            source2.OnNext(4);
            Assert.That(result, Is.EqualTo(new[] { 3, 4 }));

            source2.OnCompleted();
            Assert.That(isCompleted, Is.True);
        }
    }
}