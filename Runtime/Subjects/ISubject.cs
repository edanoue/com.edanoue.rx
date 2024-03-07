// Copyright Edanoue, Inc. All Rights Reserved.

#nullable enable
using System;

namespace Edanoue.Rx
{
    public interface ISubject<T>
    {
        // Observable
        IDisposable Subscribe(Observer<T> observer);

        // Observer
        void OnNext(T value);
        void OnErrorResume(Exception error);
        void OnCompleted(Result complete);
    }

    public static class SubjectExtensions
    {
        public static Observer<T> AsObserver<T>(this ISubject<T> subject)
        {
            return new SubjectToObserver<T>(subject);
        }

        public static void OnCompleted<T>(this ISubject<T> subject)
        {
            subject.OnCompleted(default);
        }
    }

    internal sealed class SubjectToObserver<T> : Observer<T>
    {
        private readonly ISubject<T> _subject;

        public SubjectToObserver(ISubject<T> subject)
        {
            _subject = subject;
        }

        protected override void OnNextCore(T value)
        {
            _subject.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            _subject.OnErrorResume(error);
        }

        protected override void OnCompletedCore(Result result)
        {
            _subject.OnCompleted(result);
        }
    }
}