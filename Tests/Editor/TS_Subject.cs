// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable

using System;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Edanoue.Rx
{
    public sealed class TS_Subject
    {
        [Test]
        [Category("Normal")]
        public void OnNextが動作する()
        {
            var result = 0;
            using var subject = new Subject<int>();
            subject.Subscribe(x => result = x);

            subject.OnNext(0);
            Assert.That(result, Is.EqualTo(0));

            subject.OnNext(1);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        [Category("Normal")]
        public void CompleteしたSubjectは購読するとすぐにOnCompletedが呼ばれる()
        {
            var result = 0;
            using var subject = new Subject<int>();

            // OnCompleted な Subject を Subscribe すると すぐに OnCompleted が呼ばれる
            subject.OnCompleted();
            subject.Subscribe(x => { }, r => { result = 1; });
            Assert.That(result, Is.EqualTo(1));

            // 再度 OnCompleted してもエラーは発生しない(特に意味がない)
            subject.OnCompleted();
        }

        [Test]
        [Category("Normal")]
        public void 複数の購読を作成できる()
        {
            var result = 0;
            using var subject = new Subject<Unit>();

            // 2つの購読を作成
            subject.Subscribe(_ => { result++; });
            subject.Subscribe(_ => { result++; });

            // OnNext を呼ぶと2つの購読が呼ばれる
            subject.OnNext(Unit.Default);
            Assert.That(result, Is.EqualTo(2));

            // 新たに追加することができる
            subject.Subscribe(_ => { result++; });
            subject.OnNext(Unit.Default);
            Assert.That(result, Is.EqualTo(5));
        }

        [Test]
        [Category("Normal")]
        public void 購読を解除できる()
        {
            var result = 0;
            using var subject = new Subject<Unit>();

            // 2つの購読を作成して OnNext を呼ぶ (result: 2)
            var subscriptionA = subject.Subscribe(_ => { result++; });
            var subscriptionB = subject.Subscribe(_ => { result++; });
            subject.OnNext(Unit.Default);

            // 片方を購読解除して OnNext を呼ぶと片方の購読しか呼ばれない
            subscriptionA.Dispose();
            subject.OnNext(Unit.Default);
            Assert.That(result, Is.EqualTo(3));

            // すべて購読解除する
            subscriptionB.Dispose();
            subject.OnNext(Unit.Default);
            Assert.That(result, Is.EqualTo(3));

            // 購読解除済みのものを Dispose してもエラーは発生しない(特に意味がない)
            subscriptionB.Dispose();
        }

        [Test]
        [Category("Abnormal")]
        public void DisposeしたSubjectは購読するとエラーが出る()
        {
            var subject = new Subject<int>();
            subject.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { subject.Subscribe(x => _ = x); });
        }

        [Test]
        [Category("Abnormal")]
        public void DisposeしたSubjectはOnNextを呼ぶとエラーが出る()
        {
            var subject = new Subject<int>();
            subject.Dispose();
            Assert.Throws<ObjectDisposedException>(() => { subject.OnNext(0); });
        }
    }
}