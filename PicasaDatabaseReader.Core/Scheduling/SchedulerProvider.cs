using System.Reactive.Concurrency;

namespace PicasaDatabaseReader.Core.Scheduling
{
    public class SchedulerProvider: ISchedulerProvider
    {
        public IScheduler ThreadPool => ThreadPoolScheduler.Instance;
    }
}