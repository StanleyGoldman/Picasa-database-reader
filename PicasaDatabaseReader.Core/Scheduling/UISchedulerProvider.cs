using System.Reactive.Concurrency;

namespace PicasaDatabaseReader.Core.Scheduling
{
    public sealed class UISchedulerProvider : SchedulerProvider, IUISchedulerProvider
    {
        public IScheduler CurrentThread => Scheduler.CurrentThread;

        public IScheduler Immediate => Scheduler.Immediate;

        public IScheduler NewThread => NewThreadScheduler.Default;

        public IScheduler TaskPool => TaskPoolScheduler.Default;
    }
}