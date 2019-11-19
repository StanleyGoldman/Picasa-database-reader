using System.Reactive.Concurrency;

namespace PicasaDatabaseReader.Core.Scheduling
{
    public interface ISchedulerProvider
    {
        IScheduler ThreadPool { get; }
    }
}