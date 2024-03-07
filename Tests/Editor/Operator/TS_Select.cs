// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using NUnit.Framework;

namespace Edanoue.Rx
{
    public sealed class TS_Select
    {
        [Test]
        public void Select()
        {
            using var subject = new Subject<int>();
            var result = 0;
            using var subscription = subject
                .Select(x => x * 2)
                .Subscribe(x => result = x);

            subject.OnNext(10);
            Assert.That(result, Is.EqualTo(20));

            subject.OnNext(20);
            Assert.That(result, Is.EqualTo(40));

            subject.OnNext(40);
            Assert.That(result, Is.EqualTo(80));
        }

        [Test]
        public void WhereSelect()
        {
            using var subject = new Subject<int>();
            var result = 0;
            using var subscription = subject
                .Where(x => x % 2 == 0)
                .Select(x => x * 2).Subscribe(x => result = x);

            subject.OnNext(10);
            Assert.That(result, Is.EqualTo(20));

            subject.OnNext(11);
            Assert.That(result, Is.EqualTo(20));

            subject.OnNext(20);
            Assert.That(result, Is.EqualTo(40));

            subject.OnNext(40);
            Assert.That(result, Is.EqualTo(80));

            subject.OnNext(99);
            Assert.That(result, Is.EqualTo(80));
        }

        [Test]
        public void SelectWithIndex()
        {
            using var subject = new Subject<int>();
            var result = 0;
            using var subscription = subject
                .Select((x, i) => x * 2 + i)
                .Subscribe(x => result = x);

            subject.OnNext(10);
            Assert.That(result, Is.EqualTo(20));

            subject.OnNext(20);
            Assert.That(result, Is.EqualTo(41));

            subject.OnNext(40);
            Assert.That(result, Is.EqualTo(82));
        }

        [Test]
        public void SelectState()
        {
            using var subject = new Subject<int>();
            var result = "";
            using var subscription = subject
                .Select("a", (x, state) => x * 2 + state)
                .Subscribe(x => result = x);

            subject.OnNext(10);
            Assert.That(result, Is.EqualTo("20a"));

            subject.OnNext(20);
            Assert.That(result, Is.EqualTo("40a"));

            subject.OnNext(40);
            Assert.That(result, Is.EqualTo("80a"));
        }
    }
}