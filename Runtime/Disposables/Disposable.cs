// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;

namespace Edanoue.Rx
{
    public static class Disposable
    {
        public static readonly IDisposable Empty = new EmptyDisposable();

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static DisposableBuilder CreateBuilder()
        {
            return new DisposableBuilder();
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="builder"></param>
        public static void AddTo(this IDisposable disposable, ref DisposableBuilder builder)
        {
            builder.Add(disposable);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="bag"></param>
        public static void AddTo(this IDisposable disposable, ref DisposableBag bag)
        {
            bag.Add(disposable);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="disposables"></param>
        public static void AddTo(this IDisposable disposable, ICollection<IDisposable> disposables)
        {
            disposables.Add(disposable);
        }

        /// <summary>
        /// Adds the specified IDisposable object to the CancellationToken
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static CancellationTokenRegistration AddTo(this IDisposable disposable,
            CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                throw new ArgumentException("Require CancellationToken CanBeCanceled");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                disposable.Dispose();
                return default;
            }

            return cancellationToken.UnsafeRegister(state =>
            {
                var d = (IDisposable)state!;
                d.Dispose();
            }, disposable);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposables"></param>
        /// <returns></returns>
        public static IDisposable Combine(params IDisposable[] disposables)
        {
            return new CombinedDisposable(disposables);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2)
        {
            return new CombinedDisposable2(disposable1, disposable2);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <param name="disposable3"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
        {
            return new CombinedDisposable3(disposable1, disposable2, disposable3);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <param name="disposable3"></param>
        /// <param name="disposable4"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4)
        {
            return new CombinedDisposable4(disposable1, disposable2, disposable3, disposable4);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <param name="disposable3"></param>
        /// <param name="disposable4"></param>
        /// <param name="disposable5"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5)
        {
            return new CombinedDisposable5(disposable1, disposable2, disposable3, disposable4, disposable5);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <param name="disposable3"></param>
        /// <param name="disposable4"></param>
        /// <param name="disposable5"></param>
        /// <param name="disposable6"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5, IDisposable disposable6)
        {
            return new CombinedDisposable6(disposable1, disposable2, disposable3, disposable4, disposable5,
                disposable6);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <param name="disposable3"></param>
        /// <param name="disposable4"></param>
        /// <param name="disposable5"></param>
        /// <param name="disposable6"></param>
        /// <param name="disposable7"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7)
        {
            return new CombinedDisposable7(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6,
                disposable7);
        }

        /// <summary>
        /// </summary>
        /// <param name="disposable1"></param>
        /// <param name="disposable2"></param>
        /// <param name="disposable3"></param>
        /// <param name="disposable4"></param>
        /// <param name="disposable5"></param>
        /// <param name="disposable6"></param>
        /// <param name="disposable7"></param>
        /// <param name="disposable8"></param>
        /// <returns></returns>
        public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7,
            IDisposable disposable8)
        {
            return new CombinedDisposable8(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6,
                disposable7, disposable8);
        }
    }

    internal sealed class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}