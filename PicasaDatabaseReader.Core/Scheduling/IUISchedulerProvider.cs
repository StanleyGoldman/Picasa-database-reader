using System.Reactive.Concurrency;

namespace PicasaDatabaseReader.Core.Scheduling
{
    public interface IUISchedulerProvider : ISchedulerProvider
    {
        IScheduler CurrentThread { get; }
        IScheduler Immediate { get; }
        IScheduler NewThread { get; }
        IScheduler TaskPool { get; }
    }
}