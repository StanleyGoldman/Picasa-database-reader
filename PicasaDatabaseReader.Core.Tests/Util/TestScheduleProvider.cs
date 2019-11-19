using System.Reactive.Concurrency;
using Microsoft.Reactive.Testing;
using PicasaDatabaseReader.Core.Scheduling;

namespace PicasaDatabaseReader.Core.Tests.Util
{
    public sealed class TestScheduleProvider : IUISchedulerProvider
    {
        public TestScheduler CurrentThread { get; } = new TestScheduler();
        public TestScheduler Immediate { get; } = new TestScheduler();
        public TestScheduler NewThread { get; } = new TestScheduler();
        public TestScheduler ThreadPool { get; } = new TestScheduler();
        public TestScheduler TaskPool { get; } = new TestScheduler();

        IScheduler IUISchedulerProvider.CurrentThread => CurrentThread;
        IScheduler IUISchedulerProvider.Immediate => Immediate;
        IScheduler IUISchedulerProvider.NewThread => NewThread;
        IScheduler ISchedulerProvider.ThreadPool => ThreadPool;
        IScheduler IUISchedulerProvider.TaskPool => TaskPool;
    }
}