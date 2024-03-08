// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_ReactiveProperty
    {
        [Test]
        public void Test()
        {
            var rp = new ReactiveProperty<int>(100);
            Assert.That(rp.Value, Is.EqualTo(100));

            rp.Value = 9999;
            Assert.That(rp.Value, Is.EqualTo(9999));

            rp.Dispose();
            Assert.That(rp.IsDisposed, Is.True);
        }

        [Test]
        public void Test1()
        {
            var counter = 0;
            using var rp = new ReactiveProperty<int>(100);
            using var s = rp.Subscribe(_ => counter++);
            Assert.That(counter, Is.EqualTo(1));

            rp.Value = 9999;
            Assert.That(counter, Is.EqualTo(2));

            rp.Value = 9999;
            Assert.That(counter, Is.EqualTo(2));

            rp.Value = 200;
            Assert.That(counter, Is.EqualTo(3));
        }

        public void DefaultValueTest()
        {
            using var rp = new ReactiveProperty<int>();
            Assert.That(rp.Value, Is.EqualTo(default(int)));
        }

        [Test]
        public void SubscribeAfterCompleted()
        {
            var rp = new ReactiveProperty<string>("foo");
            rp.OnCompleted();

            var value = "";
            using var s = rp.Subscribe(x => value = x);
            Assert.That(value, Is.EqualTo("foo"));
        }
    }
}